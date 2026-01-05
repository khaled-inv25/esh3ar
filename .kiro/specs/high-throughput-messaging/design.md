# Design Document

## Overview

The high-throughput messaging system is designed to handle 10,000+ concurrent individual API calls from business clients while delivering messages to mobile users in real-time via SignalR with zero message loss. The architecture emphasizes horizontal scalability, atomic transaction guarantees, and fault tolerance through event-driven patterns and intelligent load distribution.

The system follows a multi-layered approach with clear separation between API ingestion, message processing, and delivery layers. Each layer can scale independently based on load patterns, ensuring optimal resource utilization and performance under varying traffic conditions.

## Architecture

The system employs a distributed, event-driven architecture with the following key principles:

### Layered Architecture
- **API Layer**: Handles incoming business client requests with load balancing
- **Caching Layer**: Redis-based distributed caching for performance optimization
- **Ingestion Layer**: Processes and queues messages with atomic guarantees
- **Processing Layer**: Routes messages and manages delivery workflows
- **Delivery Layer**: Manages SignalR connections and real-time delivery
- **Storage Layer**: Provides durable persistence and queue management

### Caching Strategy
- **Connection Caching**: Cache active SignalR connections and user online status
- **Message Caching**: Cache frequently accessed messages and delivery status
- **Load Balancer Caching**: Cache node metrics and routing decisions
- **User State Caching**: Cache mobile user presence and pending message counts
- **Configuration Caching**: Cache system configuration and routing rules

### Event-Driven Communication
- Asynchronous message processing using high-performance queues
- Event sourcing for message state transitions
- Distributed event bus for cross-service communication
- Circuit breaker patterns for fault isolation
- Cache invalidation events for consistency

### Horizontal Scaling
- Stateless service design for easy horizontal scaling
- Load balancer with intelligent request distribution
- Auto-scaling based on queue depth and system metrics
- Connection pooling and resource optimization
- Distributed caching with Redis clustering

## Components and Interfaces

### Existing Components (Current Esh3arTech Architecture)

The high-throughput messaging system builds upon the existing Esh3arTech infrastructure:

#### Current Message Domain
```csharp
// Existing: src/Esh3arTech.Domain/Messages/Message.cs
public class Message : FullAuditedAggregateRoot<Guid>
{
    // Current properties: RecipientPhoneNumber, Subject, MessageContent, Status, Type, etc.
    // Existing retry logic: RetryCount, LastRetryAt, NextRetryAt, MovedToDlqAt
    // Current methods: SetMessageStatusType, IncrementRetryCount, ScheduleNextRetry
}

// Existing: src/Esh3arTech.Domain/Messages/SendBehavior/IMessageFactory.cs
public interface IMessageFactory
{
    IOneWayMessageManager Create(MessageType type);
}
```

#### Current Application Services
```csharp
// Existing: src/Esh3arTech.Application/Messages/MessageAppService.cs
public class MessageAppService : Esh3arTechAppService, IMessageAppService
{
    // Current methods: SendOneWayMessageAsync, IngestionSendOneWayMessageAsync
    // Existing dependencies: IMessageFactory, IDistributedEventBus, IMessageRepository, IMessageBuffer
}

// Existing: src/Esh3arTech.Application/Messages/Buffer/IMessageBuffer.cs
public interface IMessageBuffer
{
    ChannelWriter<Message> Writer { get; }
    ChannelReader<Message> Reader { get; }
}
```

#### Current SignalR Infrastructure
```csharp
// Existing: src/Esh3arTech.Web/Hubs/OnlineMobileUserHub.cs
[Authorize]
[HubRoute("online-mobile-user")]
public class OnlineMobileUserHub : AbpHub
{
    // Current methods: OnConnectedAsync, OnDisconnectedAsync, AcknowledgeMessage
    // Existing dependencies: OnlineUserTrackerService, IMessageAppService, IDistributedCache
}

// Existing: src/Esh3arTech.Web/MobileUsers/OnlineUserTrackerService.cs
public class OnlineUserTrackerService : ITransientDependency
{
    // Current methods: AddConnection, RemoveConnection, GetFirstConnectionIdByPhoneNumberAsync
    // Uses: IDistributedCache<MobileUserConnectionCacheItem>
}
```

### Enhanced Components for High-Throughput

#### Enhanced Load Balancer (New)
```csharp
public interface IHighThroughputLoadBalancer
{
    Task<IProcessingNode> SelectOptimalNodeAsync(LoadBalancingStrategy strategy);
    Task RegisterNodeAsync(IProcessingNode node);
    Task UnregisterNodeAsync(string nodeId);
    Task<LoadMetrics> GetRealTimeLoadMetricsAsync();
    Task<bool> CanHandleLoadAsync(int concurrentRequests);
}

public class ProcessingNode
{
    public string Id { get; set; }
    public string Endpoint { get; set; }
    public NodeStatus Status { get; set; }
    public LoadMetrics CurrentLoad { get; set; }
    public DateTime LastHeartbeat { get; set; }
    public int MaxConcurrentRequests { get; set; }
    public int CurrentActiveRequests { get; set; }
}
```

#### Enhanced Message Buffer (Extends Existing)
```csharp
// Extends existing IMessageBuffer with high-throughput capabilities
public interface IHighThroughputMessageBuffer : IMessageBuffer
{
    Task<bool> TryWriteAsync(Message message, CancellationToken cancellationToken = default);
    Task<IEnumerable<Message>> ReadBatchAsync(int batchSize, CancellationToken cancellationToken = default);
    Task<BufferMetrics> GetMetricsAsync();
    Task<bool> IsNearCapacityAsync(double threshold = 0.8);
}

public class BufferMetrics
{
    public int CurrentDepth { get; set; }
    public int MaxCapacity { get; set; }
    public double UtilizationPercentage { get; set; }
    public TimeSpan AverageProcessingTime { get; set; }
    public int MessagesPerSecond { get; set; }
}
```

#### Enhanced SignalR Hub (Extends Existing)
```csharp
// Extends existing OnlineMobileUserHub with high-throughput capabilities
public interface IHighThroughputSignalRHub
{
    Task SendMessageToUserAsync(string userId, object message);
    Task SendBatchMessagesToUserAsync(string userId, IEnumerable<object> messages);
    Task SendMessageToMultipleUsersAsync(IEnumerable<string> userIds, object message);
    Task<bool> IsUserConnectedAsync(string userId);
    Task<int> GetActiveConnectionCountAsync();
    Task<HubMetrics> GetHubMetricsAsync();
}

public class HubMetrics
{
    public int ActiveConnections { get; set; }
    public int MessagesPerSecond { get; set; }
    public TimeSpan AverageDeliveryLatency { get; set; }
    public int FailedDeliveries { get; set; }
    public DateTime LastUpdated { get; set; }
}
```

#### Enhanced Connection Management (Extends Existing)
```csharp
// Extends existing OnlineUserTrackerService with high-throughput capabilities
public interface IHighThroughputConnectionManager
{
    Task<bool> IsUserOnlineAsync(string phoneNumber);
    Task<IEnumerable<string>> GetActiveConnectionsAsync(string phoneNumber);
    Task RegisterConnectionAsync(string phoneNumber, string connectionId);
    Task UnregisterConnectionAsync(string phoneNumber, string connectionId);
    Task<ConnectionMetrics> GetConnectionMetricsAsync();
    Task<bool> CanAcceptNewConnectionsAsync();
    Task RedistributeConnectionsAsync();
}

public class ConnectionMetrics
{
    public int TotalActiveConnections { get; set; }
    public int MaxConcurrentConnections { get; set; }
    public double ConnectionUtilization { get; set; }
    public TimeSpan AverageConnectionDuration { get; set; }
    public int ConnectionsPerSecond { get; set; }
}
```

### Enhanced Caching Layer (Builds on Existing Redis)
```csharp
// Builds upon existing IDistributedCache usage in OnlineUserTrackerService
public interface IHighPerformanceCache
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task<Dictionary<string, T?>> GetBatchAsync<T>(IEnumerable<string> keys);
    Task SetBatchAsync<T>(Dictionary<string, T> items, TimeSpan? expiration = null);
    Task InvalidatePatternAsync(string pattern);
    Task<CacheMetrics> GetMetricsAsync();
}

public interface IMessageCache
{
    Task<Message?> GetMessageAsync(Guid messageId);
    Task CacheMessageAsync(Message message, TimeSpan expiration);
    Task<IEnumerable<Message>> GetPendingMessagesAsync(string phoneNumber);
    Task CachePendingMessagesAsync(string phoneNumber, IEnumerable<Message> messages);
    Task InvalidateUserMessagesAsync(string phoneNumber);
}
```

### Atomic Transaction Manager (New)
```csharp
public interface IAtomicMessagePersistence
{
    Task<TransactionResult> PersistMessageAtomicallyAsync(Message message);
    Task<bool> ValidateTransactionConsistencyAsync(Guid messageId);
    Task<IEnumerable<Message>> GetInconsistentMessagesAsync();
    Task RepairInconsistentMessageAsync(Guid messageId);
}

public class TransactionResult
{
    public bool Success { get; set; }
    public Guid MessageId { get; set; }
    public string TransactionId { get; set; }
    public bool DatabasePersisted { get; set; }
    public bool QueueInserted { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### Message Queue System
```csharp
public interface IHighPerformanceMessageQueue
{
    Task<bool> EnqueueAsync(Message message, QueuePriority priority = QueuePriority.Normal);
    Task<Message> DequeueAsync(CancellationToken cancellationToken);
    Task<IEnumerable<Message>> DequeueBatchAsync(int batchSize, CancellationToken cancellationToken);
    Task<bool> AcknowledgeAsync(string messageId);
    Task<bool> RejectAsync(string messageId, string reason);
    Task<QueueMetrics> GetMetricsAsync();
}

public interface IAtomicMessagePersistence
{
    Task<TransactionResult> PersistMessageAtomicallyAsync(Message message);
    Task<bool> MarkAsProcessedAsync(string messageId);
    Task<IEnumerable<Message>> GetUnprocessedMessagesAsync();
}
```

### Message Processing and Routing
```csharp
public interface IMessageProcessor
{
    Task ProcessMessageAsync(Message message);
    Task ProcessBatchAsync(IEnumerable<Message> messages);
    Task<ProcessingResult> ValidateAndRouteAsync(Message message);
}

public interface IMessageRouter
{
    Task<RoutingResult> RouteToMobileUserAsync(Message message, string mobileUserId);
    Task<IEnumerable<string>> GetActiveConnectionsAsync(string mobileUserId);
    Task<bool> IsUserOnlineAsync(string mobileUserId);
}
```

### SignalR Hub and Connection Management
```csharp
public interface ISignalRMessageHub
{
    Task SendMessageToUserAsync(string userId, object message);
    Task SendMessageToGroupAsync(string groupName, object message);
    Task SendBatchMessagesAsync(string userId, IEnumerable<object> messages);
}

public interface IConnectionManager
{
    Task<bool> IsConnectedAsync(string userId);
    Task<IEnumerable<string>> GetConnectionIdsAsync(string userId);
    Task RegisterConnectionAsync(string userId, string connectionId);
    Task UnregisterConnectionAsync(string connectionId);
    Task<ConnectionMetrics> GetConnectionMetricsAsync();
}
```

### Circuit Breaker and Fault Tolerance
```csharp
public interface ICircuitBreaker
{
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation);
    CircuitBreakerState GetState();
    Task ResetAsync();
    event EventHandler<CircuitBreakerStateChangedEventArgs> StateChanged;
}

public interface IFaultTolerantDelivery
{
    Task<DeliveryResult> DeliverWithRetryAsync(Message message, RetryPolicy policy);
    Task QueueForRetryAsync(Message message, TimeSpan delay);
    Task MoveToDeadLetterQueueAsync(Message message, string reason);
}
```

## Data Models

### Caching Strategy Details

#### Connection State Caching
- **User Online Status**: Cache with 30-second TTL, updated on connect/disconnect
- **Active Connections**: Cache connection IDs per user with 60-second TTL
- **Connection Metrics**: Cache hub load metrics with 10-second TTL
- **Offline Message Queue**: Cache pending message counts per user

#### Message Caching
- **Recent Messages**: Cache last 100 messages per user with 5-minute TTL
- **Delivery Status**: Cache message delivery status with 1-hour TTL
- **Failed Messages**: Cache retry queue status with 30-second TTL
- **Message Metadata**: Cache routing information with 10-minute TTL

#### Load Balancer Caching
- **Node Health**: Cache node status and metrics with 15-second TTL
- **Routing Decisions**: Cache load balancing decisions with 5-second TTL
- **Capacity Metrics**: Cache system capacity information with 30-second TTL

#### Cache Invalidation Strategy
- **Event-Driven**: Invalidate cache on state changes (connect/disconnect, message delivery)
- **Time-Based**: Use appropriate TTL values based on data volatility
- **Manual**: Provide cache invalidation APIs for administrative operations
- **Distributed**: Use Redis pub/sub for cache invalidation across nodes

### Core Message Model (Existing + Extensions)

The existing `Message` entity in `src/Esh3arTech.Domain/Messages/Message.cs` already provides:
- Retry logic: `RetryCount`, `LastRetryAt`, `NextRetryAt`
- Status management: `Status`, `SetMessageStatusType()`
- Priority handling: `Priority`, `SetPriority()`
- Attachment support: `Attachments`, `AddAttachment()`
- Dead letter queue support: `MovedToDlqAt`

```csharp
// Existing Message entity (no changes needed)
[Table(Esh3arTechConsts.TblMessage)]
public class Message : FullAuditedAggregateRoot<Guid>
{
    public string RecipientPhoneNumber { get; private set; }
    public string Subject { get; private set; }
    public string? MessageContent { get; private set; }
    public MessageStatus Status { get; private set; }
    public MessageType Type { get; private set; }
    public Priority Priority { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? LastRetryAt { get; private set; }
    public DateTime? NextRetryAt { get; private set; }
    public DateTime? MovedToDlqAt { get; private set; }
    public string? FailureReason { get; private set; }
    // ... existing methods
}

// Existing enums (already defined in Domain.Shared)
public enum MessageStatus { Queued, Processing, Delivered, Failed, DeadLetter }
public enum MessageType { OneWay, TwoWay }
public enum Priority { Low, Normal, High, Critical }
```

### High-Throughput Processing Models (New)
```csharp
public class ProcessingNode
{
    public string Id { get; set; }
    public string Endpoint { get; set; }
    public NodeStatus Status { get; set; }
    public LoadMetrics CurrentLoad { get; set; }
    public DateTime LastHeartbeat { get; set; }
    public int MaxConcurrentRequests { get; set; }
    public int CurrentActiveRequests { get; set; }
    public Dictionary<string, object> Capabilities { get; set; }
}

public class LoadMetrics
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public int ActiveConnections { get; set; }
    public int QueueDepth { get; set; }
    public double RequestsPerSecond { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public DateTime LastUpdated { get; set; }
}

public enum NodeStatus
{
    Healthy,
    Degraded,
    Unhealthy,
    Offline
}
```

### Connection and Delivery Models (Existing + Extensions)

Building upon existing infrastructure in `OnlineUserTrackerService` and `OnlineMobileUserHub`:

```csharp
// Existing: MobileUserConnectionCacheItem (referenced in OnlineUserTrackerService)
// Current structure handles connection tracking per mobile number

// Enhanced connection model for high-throughput scenarios
public class HighThroughputConnectionInfo
{
    public string PhoneNumber { get; set; }
    public List<string> ConnectionIds { get; set; } = new();
    public DateTime LastActivity { get; set; }
    public ConnectionStatus Status { get; set; }
    public int PendingMessageCount { get; set; }
    public DateTime ConnectedAt { get; set; }
    public string? UserAgent { get; set; }
}

// Extends existing delivery tracking
public class DeliveryResult
{
    public bool Success { get; set; }
    public Guid MessageId { get; set; }
    public DateTime DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DeliveryChannel Channel { get; set; }
    public TimeSpan DeliveryLatency { get; set; }
    public string? ConnectionId { get; set; }
    public int RetryAttempt { get; set; }
}

// Transaction result for atomic operations
public class TransactionResult
{
    public bool Success { get; set; }
    public Guid MessageId { get; set; }
    public string TransactionId { get; set; }
    public bool DatabasePersisted { get; set; }
    public bool QueueInserted { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }
}

// Cache entry wrapper for performance optimization
public class CacheEntry<T>
{
    public T Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Version { get; set; }
    public int HitCount { get; set; }
}

public enum DeliveryChannel
{
    SignalR,
    Fallback,
    Retry
}

public enum ConnectionStatus
{
    Connected,
    Disconnected,
    Reconnecting,
    Idle
}
```

## Error Handling

### Error Classification
- **Transient Errors**: Network timeouts, temporary service unavailability
- **Permanent Errors**: Invalid user IDs, malformed messages
- **System Errors**: Database failures, memory exhaustion
- **Business Errors**: Rate limiting, quota exceeded

### Error Handling Strategy
```csharp
public class ErrorHandlingStrategy
{
    public async Task<ErrorHandlingResult> HandleErrorAsync(Exception error, Message message)
    {
        return error switch
        {
            TransientException => await HandleTransientErrorAsync(message),
            PermanentException => await HandlePermanentErrorAsync(message),
            SystemException => await HandleSystemErrorAsync(message),
            BusinessException => await HandleBusinessErrorAsync(message),
            _ => await HandleUnknownErrorAsync(message)
        };
    }
    
    private async Task<ErrorHandlingResult> HandleTransientErrorAsync(Message message)
    {
        // Implement exponential backoff retry
        var delay = CalculateExponentialBackoff(message.RetryCount);
        await _faultTolerantDelivery.QueueForRetryAsync(message, delay);
        return ErrorHandlingResult.Retry;
    }
    
    private async Task<ErrorHandlingResult> HandlePermanentErrorAsync(Message message)
    {
        // Move to dead letter queue immediately
        await _faultTolerantDelivery.MoveToDeadLetterQueueAsync(message, "Permanent error");
        return ErrorHandlingResult.DeadLetter;
    }
}
```

### Circuit Breaker Implementation
- **Closed State**: Normal operation, requests flow through
- **Open State**: Failures detected, requests fail fast
- **Half-Open State**: Testing if service has recovered
- **Failure Threshold**: 5 consecutive failures or 50% failure rate
- **Recovery Timeout**: 30 seconds before attempting recovery

