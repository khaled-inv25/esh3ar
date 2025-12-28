# Design Document: Reliable Message Delivery and High-Throughput Queuing

## Overview

This design enhances the existing Esh3arTech messaging system to provide enterprise-grade reliability and high-throughput capabilities. The current implementation uses RabbitMQ for basic event distribution and SignalR for real-time delivery, but lacks critical features for production-scale messaging systems.

## Current State Analysis

### What Exists (Keep As-Is)

1. **Message Entity Structure** ✅
   - `Message` aggregate root with status tracking
   - `RetryCount`, `FailureReason`, `Priority` fields already present
   - `DeliveredAt`, `ReadAt` timestamps for tracking
   - Status enum with Pending, Sent, Delivered, Read, Queued, Failed states
   - Priority enum with Low, Normal, High, Urgent levels

2. **RabbitMQ Integration** ✅
   - ABP EventBus with RabbitMQ configured
   - `SendOneWayMessageEto` event for message distribution
   - PrefetchCount set to 100 for throughput
   - Exchange and queue configuration in appsettings.json

3. **Message Status Updater** ✅
   - `MessageStatusUpdater` service for status updates
   - Concurrency exception handling
   - Logging for status transitions

4. **SignalR Real-Time Delivery** ✅
   - `OnlineMobileUserHub` for real-time connections
   - `OnlineUserTrackerService` for connection tracking
   - `MessagesDeliveryHandler` for event processing

5. **Message Factory Pattern** ✅
   - `IMessageFactory` for creating different message types
   - `OneWayMessageManager` for message creation logic


### What Needs Enhancement

1. **Message Delivery Handler** 🔧
   - **Current**: Simple event handler with no retry logic
   - **Enhancement**: Add retry with exponential backoff
   - **Enhancement**: Add circuit breaker pattern
   - **Enhancement**: Add idempotency checking
   - **Location**: `MessagesDeliveryHandler.cs`

2. **Message Entity** 🔧
   - **Current**: Has `RetryCount` but no retry logic uses it
   - **Enhancement**: Add `IdempotencyKey` property
   - **Enhancement**: Add `NextRetryAt` property for scheduling
   - **Enhancement**: Add `LastRetryAt` property for tracking
   - **Enhancement**: Add methods: `IncrementRetryCount()`, `ScheduleNextRetry()`, `CanRetry()`
   - **Location**: `Message.cs`

3. **RabbitMQ Configuration** 🔧
   - **Current**: Basic exchange and queue setup
   - **Enhancement**: Add dead letter exchange configuration
   - **Enhancement**: Add priority queue configuration
   - **Enhancement**: Add message TTL and retry delays
   - **Location**: `Esh3arTechWebModule.cs` RabbitMQ configuration


4. **Caching for Pending Messages** 🔧
   - **Current**: Uses distributed cache for offline users
   - **Enhancement**: Add deduplication cache with TTL
   - **Enhancement**: Add circuit breaker state cache
   - **Location**: `MessagesDeliveryHandler.cs`

### What Needs to Be Created (New Components)

1. **Retry Policy Service** 🆕
   - Calculate exponential backoff delays
   - Determine if message can be retried
   - Configure retry limits and delays
   - **New File**: `src/Esh3arTech.Domain/Messages/RetryPolicy/MessageRetryPolicy.cs`

2. **Dead Letter Queue Manager** 🆕
   - Move failed messages to DLQ
   - Retrieve messages from DLQ
   - Requeue messages from DLQ
   - **New File**: `src/Esh3arTech.Application/Messages/DeadLetterQueueManager.cs`

3. **Idempotency Service** 🆕
   - Check if message already processed
   - Store processed message IDs
   - Clean up expired entries
   - **New File**: `src/Esh3arTech.Domain/Messages/Idempotency/IdempotencyService.cs`


4. **Circuit Breaker Service** 🆕
   - Track failure rates
   - Open/close circuit based on thresholds
   - Half-open state for testing recovery
   - **New File**: `src/Esh3arTech.Domain/Messages/CircuitBreaker/MessageCircuitBreaker.cs`

5. **Batch Message Processor** 🆕
   - Validate batch of messages
   - Queue messages in parallel
   - Return batch results
   - **New File**: `src/Esh3arTech.Application/Messages/BatchMessageProcessor.cs`

6. **Message Acknowledgment Service** 🆕
   - Handle delivery acknowledgments from mobile clients
   - Update message status on acknowledgment
   - Track acknowledgment timeouts
   - **New File**: `src/Esh3arTech.Application/Messages/MessageAcknowledgmentService.cs`

7. **Metrics Collector** 🆕
   - Track throughput metrics
   - Monitor queue depths
   - Track retry and failure rates
   - **New File**: `src/Esh3arTech.Domain/Messages/Metrics/MessageMetricsCollector.cs`


8. **Background Worker for Retry Processing** 🆕
   - Scan for messages ready for retry
   - Requeue messages based on NextRetryAt
   - Run periodically (every 30 seconds)
   - **New File**: `src/Esh3arTech.EntityFrameworkCore/BackgroundWorkers/MessageRetryWorker.cs`

9. **API Endpoints** 🆕
   - Dead letter queue management endpoints
   - Batch message submission endpoint
   - Message acknowledgment endpoint
   - Metrics endpoint
   - **New Files**: 
     - `src/Esh3arTech.Application.Contracts/Messages/IDeadLetterQueueAppService.cs`
     - `src/Esh3arTech.Application/Messages/DeadLetterQueueAppService.cs`
     - DTOs for batch operations and DLQ management

### What Needs to Be Removed

1. **Commented-out MessageManager** ❌
   - **Current**: Old `MessageManager.cs` is commented out
   - **Action**: Delete the file entirely
   - **Location**: `src/Esh3arTech.Domain/Messages/MessageManager.cs`


## Architecture

### High-Level Flow

```
┌─────────────┐
│   Client    │
│  (Web/API)  │
└──────┬──────┘
       │ 1. Send Message
       ▼
┌─────────────────────┐
│ MessageAppService   │
│ - Validate          │
│ - Create Message    │
│ - Check Idempotency │
└──────┬──────────────┘
       │ 2. Publish Event
       ▼
┌─────────────────────┐
│   RabbitMQ Queue    │
│ - Priority Routing  │
│ - DLQ Configuration │
└──────┬──────────────┘
       │ 3. Consume Event
       ▼
┌──────────────────────────┐
│ MessagesDeliveryHandler  │
│ - Check Circuit Breaker  │
│ - Check Idempotency      │
│ - Attempt Delivery       │
└──────┬───────────────────┘
       │
       ├─ Success ──────────┐
       │                    ▼
       │            ┌──────────────┐
       │            │ Update Status│
       │            │  (Delivered) │
       │            └──────────────┘
       │
       └─ Failure ──────────┐
                            ▼
                    ┌──────────────────┐
                    │  Retry Policy    │
                    │ - Increment Count│
                    │ - Calculate Delay│
                    └────┬─────────────┘
                         │
                         ├─ Can Retry ────────┐
                         │                    ▼
                         │            ┌──────────────┐
                         │            │ Schedule     │
                         │            │ Next Retry   │
                         │            └──────────────┘
                         │
                         └─ Max Retries ──────┐
                                              ▼
                                      ┌──────────────┐
                                      │ Move to DLQ  │
                                      │ (Failed)     │
                                      └──────────────┘
```


## Components and Interfaces

### 1. Enhanced Message Entity

**Location**: `src/Esh3arTech.Domain/Messages/Message.cs`

**Changes**:
```csharp
// Add new properties
public string IdempotencyKey { get; private set; }
public DateTime? NextRetryAt { get; private set; }
public DateTime? LastRetryAt { get; private set; }

// Add new methods
public Message SetIdempotencyKey(string key)
public Message IncrementRetryCount()
public Message ScheduleNextRetry(TimeSpan delay)
public bool CanRetry(int maxRetries)
public Message MarkAsRetrying()
```

### 2. Message Retry Policy

**Location**: `src/Esh3arTech.Domain/Messages/RetryPolicy/MessageRetryPolicy.cs`

**Interface**:
```csharp
public interface IMessageRetryPolicy
{
    TimeSpan CalculateDelay(int retryCount);
    bool CanRetry(int retryCount);
    int MaxRetries { get; }
}

public class ExponentialBackoffRetryPolicy : IMessageRetryPolicy
{
    private readonly TimeSpan _baseDelay;
    private readonly TimeSpan _maxDelay;
    private readonly int _maxRetries;
    
    public TimeSpan CalculateDelay(int retryCount)
    {
        var delay = TimeSpan.FromSeconds(_baseDelay.TotalSeconds * Math.Pow(2, retryCount));
        return delay > _maxDelay ? _maxDelay : delay;
    }
}
```


### 3. Idempotency Service

**Location**: `src/Esh3arTech.Domain/Messages/Idempotency/IdempotencyService.cs`

**Interface**:
```csharp
public interface IIdempotencyService
{
    Task<bool> IsProcessedAsync(string idempotencyKey);
    Task MarkAsProcessedAsync(string idempotencyKey, TimeSpan ttl);
    Task<string> GenerateKeyAsync(Guid messageId);
}

public class IdempotencyService : IIdempotencyService
{
    private readonly IDistributedCache<IdempotencyRecord> _cache;
    
    public async Task<bool> IsProcessedAsync(string idempotencyKey)
    {
        var record = await _cache.GetAsync(idempotencyKey);
        return record != null;
    }
}
```

### 4. Circuit Breaker Service

**Location**: `src/Esh3arTech.Domain/Messages/CircuitBreaker/MessageCircuitBreaker.cs`

**Interface**:
```csharp
public interface ICircuitBreaker
{
    Task<bool> IsOpenAsync();
    Task RecordSuccessAsync();
    Task RecordFailureAsync();
    CircuitState GetState();
}

public enum CircuitState
{
    Closed,
    Open,
    HalfOpen
}
```


### 5. Enhanced Messages Delivery Handler

**Location**: `src/Esh3arTech.Web/MessagesHandler/MessagesDeliveryHandler.cs`

**Changes**:
```csharp
public class MessagesDeliveryHandler : IDistributedEventHandler<SendOneWayMessageEto>
{
    private readonly ICircuitBreaker _circuitBreaker;
    private readonly IIdempotencyService _idempotencyService;
    private readonly IMessageRetryPolicy _retryPolicy;
    private readonly IRepository<Message, Guid> _messageRepository;
    // ... existing dependencies
    
    public async Task HandleEventAsync(SendOneWayMessageEto eventData)
    {
        // 1. Check idempotency
        if (await _idempotencyService.IsProcessedAsync(eventData.IdempotencyKey))
        {
            return; // Already processed
        }
        
        // 2. Check circuit breaker
        if (await _circuitBreaker.IsOpenAsync())
        {
            await ScheduleRetryAsync(eventData);
            return;
        }
        
        // 3. Attempt delivery
        try
        {
            await DeliverMessageAsync(eventData);
            await _circuitBreaker.RecordSuccessAsync();
            await _idempotencyService.MarkAsProcessedAsync(eventData.IdempotencyKey, TimeSpan.FromHours(24));
        }
        catch (Exception ex)
        {
            await _circuitBreaker.RecordFailureAsync();
            await HandleDeliveryFailureAsync(eventData, ex);
        }
    }
}
```


### 6. Dead Letter Queue Manager

**Location**: `src/Esh3arTech.Application/Messages/DeadLetterQueueManager.cs`

**Interface**:
```csharp
public interface IDeadLetterQueueAppService
{
    Task<PagedResultDto<DeadLetterMessageDto>> GetDeadLetterMessagesAsync(PagedAndSortedResultRequestDto input);
    Task RequeueMessageAsync(Guid messageId);
    Task RequeueMultipleMessagesAsync(List<Guid> messageIds);
    Task DeleteMessageAsync(Guid messageId);
}
```

### 7. Batch Message Processor

**Location**: `src/Esh3arTech.Application/Messages/BatchMessageProcessor.cs`

**Interface**:
```csharp
public interface IBatchMessageProcessor
{
    Task<BatchMessageResultDto> SendBatchAsync(List<SendOneWayMessageDto> messages);
}

public class BatchMessageResultDto
{
    public int TotalMessages { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<MessageResultDto> Results { get; set; }
}
```


### 8. Message Acknowledgment Service

**Location**: `src/Esh3arTech.Application/Messages/MessageAcknowledgmentService.cs`

**Interface**:
```csharp
public interface IMessageAcknowledgmentService
{
    Task AcknowledgeDeliveryAsync(Guid messageId);
    Task AcknowledgeBatchAsync(List<Guid> messageIds);
}
```

### 9. Metrics Collector

**Location**: `src/Esh3arTech.Domain/Messages/Metrics/MessageMetricsCollector.cs`

**Interface**:
```csharp
public interface IMessageMetricsCollector
{
    Task RecordMessageProcessedAsync();
    Task RecordMessageFailedAsync();
    Task RecordProcessingTimeAsync(TimeSpan duration);
    Task<MessageMetricsDto> GetMetricsAsync();
}

public class MessageMetricsDto
{
    public long MessagesPerSecond { get; set; }
    public double AverageProcessingTimeMs { get; set; }
    public int QueueDepth { get; set; }
    public double RetryRate { get; set; }
    public double FailureRate { get; set; }
    public CircuitState CircuitBreakerState { get; set; }
}
```


### 10. Background Worker for Retry Processing

**Location**: `src/Esh3arTech.EntityFrameworkCore/BackgroundWorkers/MessageRetryWorker.cs`

**Implementation**:
```csharp
public class MessageRetryWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IRepository<Message, Guid> _messageRepository;
    private readonly IDistributedEventBus _eventBus;
    
    public MessageRetryWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory) 
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = 30000; // 30 seconds
    }
    
    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        // Find messages ready for retry
        var messages = await _messageRepository.GetListAsync(
            m => m.Status == MessageStatus.Failed && 
                 m.NextRetryAt <= DateTime.UtcNow &&
                 m.RetryCount < MaxRetries
        );
        
        // Republish to queue
        foreach (var message in messages)
        {
            await _eventBus.PublishAsync(MapToEto(message));
        }
    }
}
```


## Data Models

### Enhanced Message Entity

```csharp
public class Message : FullAuditedAggregateRoot<Guid>
{
    // Existing properties (keep as-is)
    public string RecipientPhoneNumber { get; private set; }
    public string Subject { get; private set; }
    public string? MessageContent { get; private set; }
    public MessageStatus Status { get; private set; }
    public MessageType Type { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public string? FailureReason { get; private set; }
    public Priority Priority { get; private set; }
    
    // New properties (add these)
    public string IdempotencyKey { get; private set; }
    public DateTime? NextRetryAt { get; private set; }
    public DateTime? LastRetryAt { get; private set; }
}
```

### New DTOs

**DeadLetterMessageDto**:
```csharp
public class DeadLetterMessageDto
{
    public Guid Id { get; set; }
    public string RecipientPhoneNumber { get; set; }
    public string MessageContent { get; set; }
    public int RetryCount { get; set; }
    public string FailureReason { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastRetryAt { get; set; }
}
```


**BatchSendMessageDto**:
```csharp
public class BatchSendMessageDto
{
    [Required]
    [MaxLength(1000)]
    public List<SendOneWayMessageDto> Messages { get; set; }
}
```

**AcknowledgeMessageDto**:
```csharp
public class AcknowledgeMessageDto
{
    public Guid MessageId { get; set; }
}

public class AcknowledgeBatchDto
{
    public List<Guid> MessageIds { get; set; }
}
```

## RabbitMQ Configuration

### Priority Queues Setup

**Location**: `src/Esh3arTech.Web/Esh3arTechWebModule.cs`

```csharp
Configure<AbpRabbitMqEventBusOptions>(options =>
{
    options.PrefetchCount = 100;
    
    // Configure priority queues
    options.ConfigureQueue("esh3artech.messages.high", queue =>
    {
        queue.Arguments.Add("x-max-priority", 10);
        queue.Durable = true;
    });
    
    options.ConfigureQueue("esh3artech.messages.normal", queue =>
    {
        queue.Arguments.Add("x-max-priority", 5);
        queue.Durable = true;
    });
    
    options.ConfigureQueue("esh3artech.messages.low", queue =>
    {
        queue.Arguments.Add("x-max-priority", 1);
        queue.Durable = true;
    });
});
```


### Dead Letter Exchange Setup

```csharp
Configure<AbpRabbitMqEventBusOptions>(options =>
{
    // Configure dead letter exchange
    options.ConfigureExchange("esh3artech.messages.dlx", exchange =>
    {
        exchange.Type = "direct";
        exchange.Durable = true;
    });
    
    // Configure dead letter queue
    options.ConfigureQueue("esh3artech.messages.dlq", queue =>
    {
        queue.Durable = true;
        queue.Arguments.Add("x-message-ttl", 86400000); // 24 hours
    });
    
    // Bind DLQ to DLX
    options.ConfigureBinding("esh3artech.messages.dlq", "esh3artech.messages.dlx", "failed");
    
    // Configure main queues with DLX
    options.ConfigureQueue("esh3artech.messages.high", queue =>
    {
        queue.Arguments.Add("x-dead-letter-exchange", "esh3artech.messages.dlx");
        queue.Arguments.Add("x-dead-letter-routing-key", "failed");
    });
});
```

## Configuration Settings

**Location**: `src/Esh3arTech.Web/appsettings.json`

```json
{
  "MessageReliability": {
    "MaxRetries": 5,
    "BaseRetryDelaySeconds": 5,
    "MaxRetryDelaySeconds": 300,
    "IdempotencyTtlHours": 24,
    "CircuitBreakerFailureThreshold": 0.5,
    "CircuitBreakerSampleSize": 10,
    "CircuitBreakerTimeoutSeconds": 30,
    "AcknowledgmentTimeoutMinutes": 5,
    "BatchSizeLimit": 1000
  }
}
```


## Error Handling

### Retry Strategy

1. **Transient Errors** (retry immediately):
   - Network timeouts
   - Connection failures
   - Temporary service unavailability

2. **Recoverable Errors** (retry with backoff):
   - Rate limiting (429)
   - Server errors (500-503)
   - Database deadlocks

3. **Permanent Errors** (move to DLQ immediately):
   - Invalid recipient (404)
   - Validation errors (400)
   - Authorization failures (401, 403)

### Error Classification

```csharp
public enum ErrorType
{
    Transient,      // Retry immediately
    Recoverable,    // Retry with backoff
    Permanent       // Move to DLQ
}

public interface IErrorClassifier
{
    ErrorType Classify(Exception exception);
}
```

## Testing Strategy

### Unit Tests

1. **Retry Policy Tests**:
   - Test exponential backoff calculation
   - Test max retry limit enforcement
   - Test delay capping at max delay

2. **Circuit Breaker Tests**:
   - Test state transitions (Closed → Open → HalfOpen → Closed)
   - Test failure threshold detection
   - Test timeout and recovery

3. **Idempotency Tests**:
   - Test duplicate detection
   - Test TTL expiration
   - Test key generation


### Integration Tests

1. **End-to-End Message Flow**:
   - Send message → Queue → Deliver → Acknowledge
   - Send message → Fail → Retry → Deliver
   - Send message → Max retries → Move to DLQ

2. **Batch Processing**:
   - Submit batch → Validate → Queue all
   - Submit batch with invalid message → Reject all

3. **Priority Queue**:
   - Send high and low priority messages
   - Verify high priority processed first

### Performance Tests

1. **Throughput Testing**:
   - Measure messages per second
   - Test with 1000, 10000, 100000 messages
   - Monitor queue depth and processing time

2. **Concurrency Testing**:
   - Multiple consumers processing simultaneously
   - Verify no duplicate processing
   - Verify message ordering per recipient

3. **Stress Testing**:
   - Circuit breaker under high failure rate
   - System behavior when queue is full
   - Recovery after RabbitMQ restart

## Monitoring and Observability

### Metrics to Track

1. **Throughput Metrics**:
   - Messages processed per second
   - Messages queued per second
   - Queue depth by priority

2. **Reliability Metrics**:
   - Retry rate (retries / total messages)
   - Failure rate (failures / total messages)
   - DLQ message count
   - Average retry count before success


3. **Performance Metrics**:
   - Average processing time
   - P95, P99 processing time
   - Circuit breaker state changes
   - Idempotency cache hit rate

4. **Health Metrics**:
   - RabbitMQ connection status
   - Database connection status
   - Cache connection status

### Logging Strategy

1. **Info Level**:
   - Message queued
   - Message delivered
   - Circuit breaker state change

2. **Warning Level**:
   - Retry scheduled
   - Circuit breaker opened
   - Acknowledgment timeout

3. **Error Level**:
   - Message moved to DLQ
   - Unhandled exceptions
   - Configuration errors

### Alerting Rules

1. **Critical Alerts**:
   - DLQ message count > 1000
   - Failure rate > 10%
   - Circuit breaker open for > 5 minutes

2. **Warning Alerts**:
   - Queue depth > 10000
   - Average processing time > 1 second
   - Retry rate > 20%

## Migration Strategy

### Phase 1: Add New Infrastructure (No Breaking Changes)

1. Add new properties to Message entity
2. Create database migration
3. Deploy new services (circuit breaker, idempotency, retry policy)
4. Configure RabbitMQ DLQ and priority queues
5. Deploy without activating new features

### Phase 2: Enable Reliability Features

1. Enable idempotency checking
2. Enable circuit breaker
3. Enable retry with exponential backoff
4. Monitor metrics and adjust thresholds

### Phase 3: Enable High-Throughput Features

1. Enable priority queues
2. Deploy batch processing endpoints
3. Enable message acknowledgment
4. Deploy retry background worker

### Rollback Plan

1. Disable new features via configuration
2. Fall back to existing MessagesDeliveryHandler logic
3. New properties in Message entity are nullable (no data loss)
4. RabbitMQ queues remain compatible

## Summary of Changes

### Files to Modify (8 files)

1. ✏️ `src/Esh3arTech.Domain/Messages/Message.cs` - Add properties and methods
2. ✏️ `src/Esh3arTech.Web/MessagesHandler/MessagesDeliveryHandler.cs` - Add reliability logic
3. ✏️ `src/Esh3arTech.Web/Esh3arTechWebModule.cs` - Configure RabbitMQ
4. ✏️ `src/Esh3arTech.Application/Messages/MessageAppService.cs` - Add idempotency
5. ✏️ `src/Esh3arTech.Application.Contracts/Messages/SendOneWayMessageEto.cs` - Add IdempotencyKey
6. ✏️ `src/Esh3arTech.Web/appsettings.json` - Add configuration
7. ✏️ `src/Esh3arTech.EntityFrameworkCore/EntityFrameworkCore/Esh3arTechDbContext.cs` - Add migration
8. ✏️ `src/Esh3arTech.Domain.Shared/Messages/MessageStatus.cs` - Add Retrying status (optional)

### Files to Create (15+ files)

1. 🆕 `src/Esh3arTech.Domain/Messages/RetryPolicy/IMessageRetryPolicy.cs`
2. 🆕 `src/Esh3arTech.Domain/Messages/RetryPolicy/ExponentialBackoffRetryPolicy.cs`
3. 🆕 `src/Esh3arTech.Domain/Messages/Idempotency/IIdempotencyService.cs`
4. 🆕 `src/Esh3arTech.Domain/Messages/Idempotency/IdempotencyService.cs`
5. 🆕 `src/Esh3arTech.Domain/Messages/CircuitBreaker/ICircuitBreaker.cs`
6. 🆕 `src/Esh3arTech.Domain/Messages/CircuitBreaker/MessageCircuitBreaker.cs`
7. 🆕 `src/Esh3arTech.Domain/Messages/Metrics/IMessageMetricsCollector.cs`
8. 🆕 `src/Esh3arTech.Domain/Messages/Metrics/MessageMetricsCollector.cs`
9. 🆕 `src/Esh3arTech.Application/Messages/BatchMessageProcessor.cs`
10. 🆕 `src/Esh3arTech.Application/Messages/DeadLetterQueueAppService.cs`
11. 🆕 `src/Esh3arTech.Application/Messages/MessageAcknowledgmentService.cs`
12. 🆕 `src/Esh3arTech.Application.Contracts/Messages/IDeadLetterQueueAppService.cs`
13. 🆕 `src/Esh3arTech.Application.Contracts/Messages/IBatchMessageProcessor.cs`
14. 🆕 `src/Esh3arTech.Application.Contracts/Messages/DeadLetterMessageDto.cs`
15. 🆕 `src/Esh3arTech.Application.Contracts/Messages/BatchSendMessageDto.cs`
16. 🆕 `src/Esh3arTech.EntityFrameworkCore/BackgroundWorkers/MessageRetryWorker.cs`

### Files to Delete (1 file)

1. ❌ `src/Esh3arTech.Domain/Messages/MessageManager.cs` - Commented out, no longer needed

### Database Migration

**New columns in Messages table**:
- `IdempotencyKey` (string, indexed)
- `NextRetryAt` (datetime, nullable, indexed)
- `LastRetryAt` (datetime, nullable)

**Indexes to add**:
- Index on `(Status, NextRetryAt)` for retry worker
- Index on `IdempotencyKey` for deduplication
- Index on `(RecipientPhoneNumber, CreationTime)` for ordering
