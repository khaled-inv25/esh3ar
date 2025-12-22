# Design Document: Guaranteed Message Delivery System

## Overview

This design document describes a high-performance, scalable messaging system that delivers one-way messages from business users to mobile users with guaranteed delivery, real-time status tracking, and enterprise-grade reliability. The system leverages RabbitMQ for message queuing, SignalR for real-time communication, distributed caching for connection state, and background workers for reliable message processing.

The architecture follows a distributed, event-driven design with clear separation between message ingestion, queuing, delivery, and monitoring concerns. The system is designed to handle 10,000+ messages per second with sub-200ms latency while maintaining 99.9% availability.

## Architecture

### High-Level Architecture

```
┌─────────────────┐
│  Business User  │
│   (Web Admin)   │
└────────┬────────┘
         │ HTTP/REST
         ▼
┌─────────────────────────────────────────────────────────────┐
│                    Web Application Layer                     │
│  ┌──────────────────┐      ┌─────────────────────────────┐ │
│  │ MessageAppService│──────▶│  MessageManager (Domain)    │ │
│  └──────────────────┘      └─────────────────────────────┘ │
└────────┬────────────────────────────────────────────────────┘
         │ Publish Event
         ▼
┌─────────────────────────────────────────────────────────────┐
│                      RabbitMQ Event Bus                      │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  SendOneWayMessageEto (Distributed Event)            │  │
│  └──────────────────────────────────────────────────────┘  │
└────────┬────────────────────────────────────────────────────┘
         │ Subscribe
         ▼
┌─────────────────────────────────────────────────────────────┐
│              Message Delivery Worker (Background)            │
│  ┌──────────────────┐      ┌─────────────────────────────┐ │
│  │ DeliveryHandler  │──────▶│  OnlineUserTrackerService   │ │
│  └──────────────────┘      └─────────────────────────────┘ │
└────────┬────────────────────────────────────────────────────┘
         │ SignalR Push
         ▼
┌─────────────────────────────────────────────────────────────┐
│                    SignalR Connection Hub                    │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  OnlineMobileUserHub (Persistent WebSocket)          │  │
│  └──────────────────────────────────────────────────────┘  │
└────────┬────────────────────────────────────────────────────┘
         │ WebSocket
         ▼
┌─────────────────┐
│   Mobile User   │
│  (Mobile App)   │
└─────────────────┘
```

### Technology Stack Alignment

- **Message Queue**: RabbitMQ (already configured in appsettings.json)
- **Real-Time Communication**: SignalR with @abp/signalr (already in package.json)
- **Background Processing**: ABP Background Jobs with Hangfire/Quartz
- **Distributed Cache**: Redis for connection state and distributed locking
- **Database**: SQL Server with Entity Framework Core
- **Blob Storage**: ABP Blob Storing (Database provider or Azure Blob Storage)
- **Monitoring**: Serilog with structured logging, Application Insights

### Deployment Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                      Load Balancer (Azure LB)                 │
└────────┬─────────────────────────────────────┬────────────────┘
         │                                     │
         ▼                                     ▼
┌─────────────────────┐            ┌─────────────────────┐
│  Web Instance 1     │            │  Web Instance 2     │
│  (SignalR Hub)      │◀──Sticky──▶│  (SignalR Hub)      │
└──────────┬──────────┘  Sessions  └──────────┬──────────┘
           │                                   │
           └───────────────┬───────────────────┘
                           │
                           ▼
                  ┌────────────────────┐
                  │   Redis Cache      │
                  │ (Connection State) │
                  └────────────────────┘
                           │
           ┌───────────────┼───────────────┐
           │               │               │
           ▼               ▼               ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│  Worker 1    │  │  Worker 2    │  │  Worker N    │
│ (Delivery)   │  │ (Delivery)   │  │ (Delivery)   │
└──────┬───────┘  └──────┬───────┘  └──────┬───────┘
       │                 │                 │
       └─────────────────┼─────────────────┘
                         │
                         ▼
                ┌─────────────────┐
                │   RabbitMQ      │
                │  (Message Bus)  │
                └─────────────────┘
                         │
                         ▼
                ┌─────────────────┐
                │  SQL Server     │
                │ (Message Store) │
                └─────────────────┘
```

## Components and Interfaces

### 1. Message Domain Entity (Enhanced)

**Location**: `src/Esh3arTech.Domain/Messages/Message.cs`

```csharp
public class Message : FullAuditedAggregateRoot<Guid>
{
    public Guid SenderId { get; set; }
    public Guid RecipientId { get; set; }
    public string Content { get; set; }
    public MessageContentType ContentType { get; set; }
    public MessageType Type { get; set; } // OneWay
    public MessageStatus Status { get; set; }
    public Priority Priority { get; set; }

    // Delivery tracking
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public int RetryCount { get; set; }
    public DateTime? LastRetryAt { get; set; }
    public string FailureReason { get; set; }

    // Media attachments
    public List<MessageAttachment> Attachments { get; set; }

    // Idempotency
    public string IdempotencyKey { get; set; }

    // Methods
    public void MarkAsSent();
    public void MarkAsDelivered();
    public void MarkAsRead();
    public void MarkAsFailed(string reason);
    public void IncrementRetryCount();
    public bool CanRetry(int maxRetries);
}

public class MessageAttachment : Entity<Guid>
{
    public Guid MessageId { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public string BlobName { get; set; }
    public string AccessUrl { get; set; }
    public DateTime? UrlExpiresAt { get; set; }
}
```

### 2. Message Manager (Domain Service)

**Location**: `src/Esh3arTech.Domain/Messages/MessageManager.cs`

```csharp
public class MessageManager : DomainService
{
    private readonly IRepository<Message, Guid> _messageRepository;
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly IBlobContainer _blobContainer;
    private readonly IDistributedCache<MessageLock> _lockCache;

    public async Task<Message> CreateOneWayMessageAsync(
        Guid senderId,
        Guid recipientId,
        string content,
        List<IFormFile> attachments = null);

    public async Task<bool> AcquireMessageLockAsync(Guid messageId, TimeSpan leaseTime);
    public async Task ReleaseMessageLockAsync(Guid messageId);
    public async Task<List<Message>> GetPendingMessagesForUserAsync(Guid userId);
    public async Task MoveToDeadLetterQueueAsync(Guid messageId, string reason);
}
```

### 3. Message Application Service

**Location**: `src/Esh3arTech.Application/Messages/MessageAppService.cs`

```csharp
public class MessageAppService : Esh3arTechAppService, IMessageAppService
{
    private readonly MessageManager _messageManager;
    private readonly IDistributedEventBus _distributedEventBus;

    [Authorize(Esh3arTechPermissions.SenderSendMessage)]
    public async Task<MessageDto> SendOneWayMessageAsync(SendOneWayMessageDto input)
    {
        // 1. Validate input and check rate limits
        // 2. Create message entity with attachments
        // 3. Persist to database
        // 4. Publish SendOneWayMessageEto event to RabbitMQ
        // 5. Return message DTO with ID and initial status
    }

    public async Task<MessageDto> GetMessageStatusAsync(Guid messageId);
    public async Task<PagedResultDto<MessageDto>> GetMessageHistoryAsync(MessageFilterDto filter);
    public async Task<List<MessageDto>> GetDeadLetterQueueAsync();
    public async Task RetryFailedMessageAsync(Guid messageId);
}
```

### 4. Online User Tracker Service

**Location**: `src/Esh3arTech.Web/MobileUsers/OnlineUserTrackerService.cs`

```csharp
public class OnlineUserTrackerService
{
    private readonly IDistributedCache<UserConnectionsCacheItem> _connectionCache;
    private readonly ILogger<OnlineUserTrackerService> _logger;

    public async Task AddConnectionAsync(Guid userId, string connectionId);
    public async Task RemoveConnectionAsync(Guid userId, string connectionId);
    public async Task<bool> IsUserOnlineAsync(Guid userId);
    public async Task<List<string>> GetUserConnectionsAsync(Guid userId);
    public async Task<int> GetOnlineUserCountAsync();
    public async Task MarkUserOfflineAsync(Guid userId);
    public async Task CleanupStaleConnectionsAsync();
}

public class UserConnectionsCacheItem
{
    public Guid UserId { get; set; }
    public List<string> ConnectionIds { get; set; }
    public DateTime LastHeartbeat { get; set; }
    public bool IsOnline { get; set; }
}
```

### 5. SignalR Hub (Enhanced)

**Location**: `src/Esh3arTech.Web/Hubs/OnlineMobileUserHub.cs`

```csharp
[Authorize]
public class OnlineMobileUserHub : AbpHub
{
    private readonly OnlineUserTrackerService _onlineTracker;
    private readonly MessageManager _messageManager;

    public override async Task OnConnectedAsync()
    {
        var userId = CurrentUser.GetId();
        await _onlineTracker.AddConnectionAsync(userId, Context.ConnectionId);

        // Trigger delivery of pending messages
        await Clients.Caller.SendAsync("ConnectionEstablished");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = CurrentUser.GetId();
        await _onlineTracker.RemoveConnectionAsync(userId, Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task AcknowledgeMessage(Guid messageId)
    {
        // Update message status to Delivered
        // Called by mobile client when message is received
    }

    public async Task MarkMessageAsRead(Guid messageId)
    {
        // Update message status to Read
        // Called by mobile client when message is opened
    }

    public async Task Heartbeat()
    {
        var userId = CurrentUser.GetId();
        await _onlineTracker.UpdateHeartbeatAsync(userId);
    }
}
```

### 6. Message Delivery Handler (Background Worker)

**Location**: `src/Esh3arTech.Web/MessagesHandler/MessageDeliveryHandler.cs`

```csharp
public class MessageDeliveryHandler :
    IDistributedEventHandler<SendOneWayMessageEto>,
    ITransientDependency
{
    private readonly OnlineUserTrackerService _onlineTracker;
    private readonly IHubContext<OnlineMobileUserHub> _hubContext;
    private readonly MessageManager _messageManager;
    private readonly IDistributedEventBus _eventBus;
    private readonly ILogger<MessageDeliveryHandler> _logger;

    public async Task HandleEventAsync(SendOneWayMessageEto eventData)
    {
        var messageId = eventData.MessageId;
        var recipientId = eventData.RecipientId;

        // 1. Acquire distributed lock for message
        if (!await _messageManager.AcquireMessageLockAsync(messageId, TimeSpan.FromSeconds(30)))
        {
            _logger.LogWarning($"Could not acquire lock for message {messageId}");
            return; // Another worker is processing this message
        }

        try
        {
            // 2. Check if user is online
            var isOnline = await _onlineTracker.IsUserOnlineAsync(recipientId);

            if (isOnline)
            {
                // 3. Deliver to online user via SignalR
                await DeliverToOnlineUserAsync(messageId, recipientId, eventData);
            }
            else
            {
                // 4. Keep in queue for offline user
                await HandleOfflineUserAsync(messageId, recipientId);
            }
        }
        catch (Exception ex)
        {
            // 5. Handle delivery failure with retry logic
            await HandleDeliveryFailureAsync(messageId, ex);
        }
        finally
        {
            // 6. Release lock
            await _messageManager.ReleaseMessageLockAsync(messageId);
        }
    }

    private async Task DeliverToOnlineUserAsync(Guid messageId, Guid recipientId, SendOneWayMessageEto eventData);
    private async Task HandleOfflineUserAsync(Guid messageId, Guid recipientId);
    private async Task HandleDeliveryFailureAsync(Guid messageId, Exception ex);
}
```

### 7. Pending Messages Delivery Worker

**Location**: `src/Esh3arTech.EntityFrameworkCore/BackgroundWorkers/PendingMessagesDeliveryWorker.cs`

```csharp
public class PendingMessagesDeliveryWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly MessageManager _messageManager;
    private readonly OnlineUserTrackerService _onlineTracker;
    private readonly IDistributedEventBus _eventBus;

    public PendingMessagesDeliveryWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = 5000; // Check every 5 seconds
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        // 1. Get list of online users
        var onlineUsers = await _onlineTracker.GetOnlineUsersAsync();

        // 2. For each online user, check for pending messages
        foreach (var userId in onlineUsers)
        {
            var pendingMessages = await _messageManager.GetPendingMessagesForUserAsync(userId);

            // 3. Publish delivery events for pending messages
            foreach (var message in pendingMessages)
            {
                await _eventBus.PublishAsync(new SendOneWayMessageEto
                {
                    MessageId = message.Id,
                    RecipientId = userId,
                    Content = message.Content,
                    // ... other properties
                });
            }
        }
    }
}
```

### 8. Retry Policy Service

**Location**: `src/Esh3arTech.Domain/Messages/RetryPolicyService.cs`

```csharp
public class RetryPolicyService : DomainService
{
    private static readonly int[] BackoffIntervals = { 1, 2, 4, 8, 16 }; // seconds
    private const int MaxRetries = 5;

    public bool ShouldRetry(Message message)
    {
        return message.RetryCount < MaxRetries;
    }

    public TimeSpan GetNextRetryDelay(int retryCount)
    {
        if (retryCount >= BackoffIntervals.Length)
            return TimeSpan.FromSeconds(BackoffIntervals[^1]);

        return TimeSpan.FromSeconds(BackoffIntervals[retryCount]);
    }

    public async Task ScheduleRetryAsync(Message message)
    {
        var delay = GetNextRetryDelay(message.RetryCount);

        // Schedule delayed event publication
        await _backgroundJobManager.EnqueueAsync(
            new RetryMessageDeliveryArgs
            {
                MessageId = message.Id
            },
            delay: delay
        );
    }
}
```

### 9. Heartbeat Monitor Worker

**Location**: `src/Esh3arTech.EntityFrameworkCore/BackgroundWorkers/HeartbeatMonitorWorker.cs`

```csharp
public class HeartbeatMonitorWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly OnlineUserTrackerService _onlineTracker;
    private readonly IHubContext<OnlineMobileUserHub> _hubContext;

    public HeartbeatMonitorWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = 30000; // Check every 30 seconds
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        // 1. Send heartbeat ping to all connected users
        await _hubContext.Clients.All.SendAsync("Ping");

        // 2. Check for users who haven't responded to last 2 heartbeats
        var staleUsers = await _onlineTracker.GetStaleConnectionsAsync(TimeSpan.FromSeconds(60));

        // 3. Mark stale users as offline
        foreach (var userId in staleUsers)
        {
            await _onlineTracker.MarkUserOfflineAsync(userId);
        }

        // 4. Cleanup connections older than 5 minutes
        await _onlineTracker.CleanupStaleConnectionsAsync();
    }
}
```

### 10. Circuit Breaker Service

**Location**: `src/Esh3arTech.Domain/Messages/CircuitBreakerService.cs`

```csharp
public class CircuitBreakerService : DomainService
{
    private readonly IDistributedCache<CircuitBreakerState> _cache;
    private const double FailureThreshold = 0.5; // 50%
    private const int MinimumThroughput = 10;
    private const int CooldownSeconds = 60;

    public async Task<bool> IsOpenAsync(string resourceName);
    public async Task RecordSuccessAsync(string resourceName);
    public async Task RecordFailureAsync(string resourceName);
    public async Task TryCloseAsync(string resourceName);
}

public class CircuitBreakerState
{
    public string ResourceName { get; set; }
    public bool IsOpen { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime WindowStart { get; set; }
}
```

## Data Models

### Message Entity Schema

```sql
CREATE TABLE EtMessages (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    SenderId UNIQUEIDENTIFIER NOT NULL,
    RecipientId UNIQUEIDENTIFIER NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    ContentType INT NOT NULL,
    Type INT NOT NULL,
    Status INT NOT NULL,
    Priority INT NOT NULL,

    -- Delivery tracking
    SentAt DATETIME2 NULL,
    DeliveredAt DATETIME2 NULL,
    ReadAt DATETIME2 NULL,
    RetryCount INT NOT NULL DEFAULT 0,
    LastRetryAt DATETIME2 NULL,
    FailureReason NVARCHAR(500) NULL,

    -- Idempotency
    IdempotencyKey NVARCHAR(100) NOT NULL,

    -- Audit fields
    CreationTime DATETIME2 NOT NULL,
    CreatorId UNIQUEIDENTIFIER NULL,
    LastModificationTime DATETIME2 NULL,
    LastModifierId UNIQUEIDENTIFIER NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeleterId UNIQUEIDENTIFIER NULL,
    DeletionTime DATETIME2 NULL,

    INDEX IX_Messages_RecipientId_Status (RecipientId, Status),
    INDEX IX_Messages_Status_CreationTime (Status, CreationTime),
    INDEX IX_Messages_IdempotencyKey (IdempotencyKey)
);

CREATE TABLE EtMessageAttachments (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    MessageId UNIQUEIDENTIFIER NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    FileSize BIGINT NOT NULL,
    BlobName NVARCHAR(500) NOT NULL,
    AccessUrl NVARCHAR(1000) NULL,
    UrlExpiresAt DATETIME2 NULL,

    FOREIGN KEY (MessageId) REFERENCES EtMessages(Id) ON DELETE CASCADE
);
```

### Redis Cache Structures

```csharp
// Connection state cache
Key: "UserConnections:{UserId}"
Value: UserConnectionsCacheItem
TTL: 10 minutes (refreshed on heartbeat)

// Message lock cache
Key: "MessageLock:{MessageId}"
Value: { WorkerId, AcquiredAt }
TTL: 30 seconds (lease time)

// Circuit breaker state cache
Key: "CircuitBreaker:{ResourceName}"
Value: CircuitBreakerState
TTL: 5 minutes

// Rate limit cache
Key: "RateLimit:{UserId}:{Minute}"
Value: MessageCount
TTL: 60 seconds
```

## Correctness Properties

A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.

### Property 1: Message Creation Returns Unique Identifiers

_For any_ set of valid message submissions, each message should receive a unique identifier that is never reused.

**Validates: Requirements 1.1, 11.1**

### Property 2: Message Persistence is Immediate

_For any_ message that is successfully queued, querying the Message_Store immediately after should return the message with all its properties intact.

**Validates: Requirements 1.2**

### Property 3: Media Validation Rejects Invalid Files

_For any_ message with media attachments, if any attachment violates type or size constraints, the entire message submission should be rejected with descriptive errors.

**Validates: Requirements 1.3, 6.2, 6.3**

### Property 4: Invalid Messages Are Not Queued

_For any_ message that fails validation, the message should not appear in Message_Store and an error should be returned to the caller.

**Validates: Requirements 1.4**

### Property 5: Initial Message State is Correct

_For any_ successfully queued message, the initial status should be "Queued" and the creation timestamp should be set to the current time (within 1 second tolerance).

**Validates: Requirements 1.5**

### Property 6: Online Users Receive Real-Time Delivery

_For any_ message sent to an online mobile user, the message should be delivered via SignalR and the status should transition to "Sent".

**Validates: Requirements 1.6, 2.3**

### Property 7: Offline Users Have Messages Queued

_For any_ message sent to an offline mobile user, the message should remain in Message_Queue with status "Pending" until the user comes online.

**Validates: Requirements 1.7, 3.1**

### Property 8: Acknowledgment Updates Status to Delivered

_For any_ message that receives an acknowledgment from the mobile user, the status should transition from "Sent" to "Delivered" and the DeliveredAt timestamp should be recorded.

**Validates: Requirements 1.9, 2.5**

### Property 9: Connection State is Tracked

_For any_ mobile user that connects to Connection_Hub, the Online_Tracker should maintain their connection state as online with at least one connection ID.

**Validates: Requirements 2.1, 3.2**

### Property 10: Timeout Triggers Retry

_For any_ message that does not receive acknowledgment within the timeout period, the system should retry delivery according to the Retry_Policy.

**Validates: Requirements 2.6**

### Property 11: Pending Messages Retrieved on Reconnection

_For any_ offline user with pending messages, when the user comes online, all pending messages for that user should be retrieved from Message_Queue.

**Validates: Requirements 3.3**

### Property 12: Messages Delivered in Chronological Order

_For any_ set of pending messages for a user, when delivered, the messages should be ordered by creation timestamp (oldest first).

**Validates: Requirements 3.4**

### Property 13: Batch Status Update on Delivery

_For any_ set of pending messages that are delivered to a user, all messages should have their status updated to "Sent".

**Validates: Requirements 3.5**

### Property 14: Status Query Returns Complete History

_For any_ message with status transitions, querying the message status should return the current status and all transition timestamps (SentAt, DeliveredAt, ReadAt).

**Validates: Requirements 4.2**

### Property 15: Only Valid State Transitions Allowed

_For any_ message, status transitions should only follow the valid path: Queued → Sent → Delivered → Read, or Queued → Failed.

**Validates: Requirements 4.3**

### Property 16: Failure Tracking is Complete

_For any_ message that fails delivery, the system should record the failure reason and increment the retry count.

**Validates: Requirements 4.4**

### Property 17: Read Status Updates Correctly

_For any_ message marked as read by a mobile user, the status should transition to "Read" and the ReadAt timestamp should be recorded.

**Validates: Requirements 4.5**

### Property 18: Exponential Backoff Retry Policy

_For any_ message that fails delivery, retries should occur with exponential backoff intervals: 1s, 2s, 4s, 8s, 16s.

**Validates: Requirements 5.1, 5.2**

### Property 19: Dead Letter Queue After Max Retries

_For any_ message that exceeds 5 retry attempts, the message should be moved to Dead_Letter_Queue with status "Failed".

**Validates: Requirements 5.3, 5.4**

### Property 20: Retry Metadata is Persisted

_For any_ message that is retried, the retry count and last retry timestamp should be persisted in Message_Store.

**Validates: Requirements 5.5**

### Property 21: Media Upload Precedes Queuing

_For any_ message with media attachments, all attachments should be uploaded to Media_Storage and have BlobName assigned before the message is queued.

**Validates: Requirements 6.1**

### Property 22: Media URLs Generated with Expiration

_For any_ uploaded media attachment, a secure access URL should be generated with an expiration timestamp set in the future.

**Validates: Requirements 6.4**

### Property 23: Message Payload Includes Media URLs

_For any_ message delivered with media attachments, the message payload should include the access URLs for all attachments.

**Validates: Requirements 6.5**

### Property 24: Circuit Breaker Opens at Threshold

_For any_ external dependency, when the failure rate exceeds 50% over a minimum of 10 requests, the circuit breaker should open.

**Validates: Requirements 8.3**

### Property 25: Distributed Locking Prevents Duplicates

_For any_ message being processed by multiple workers concurrently, only one worker should successfully acquire the lock and process the message.

**Validates: Requirements 9.3, 11.2**

### Property 26: Lock Release on Worker Failure

_For any_ message lock held by a worker that fails, the lock should be released after the lease timeout (30 seconds) allowing another worker to process it.

**Validates: Requirements 11.3**

### Property 27: Online Users Prioritized Over Offline

_For any_ delivery worker processing messages, messages for online users should be processed before messages for offline users.

**Validates: Requirements 10.4**

### Property 28: Idempotency Prevents Duplicate Delivery

_For any_ message with an idempotency key, attempting to deliver the same message multiple times should result in only one delivery.

**Validates: Requirements 11.4**

### Property 29: Acknowledgment Prevents Redelivery

_For any_ message that has been acknowledged by a mobile user, the message should not be redelivered even if the user reconnects.

**Validates: Requirements 11.5**

### Property 30: Offline Marking Rolls Back Message Status

_For any_ user marked as offline with messages in "Sent" status, those messages should transition back to "Pending" status.

**Validates: Requirements 12.3**

### Property 31: Delivery Resumes from Last Acknowledged

_For any_ user that reconnects, message delivery should resume from the first unacknowledged message in chronological order.

**Validates: Requirements 12.5**

### Property 32: Dead Letter Queue Logs Failure Details

_For any_ message moved to Dead_Letter_Queue, the system should log the failure reason, retry count, and retry history.

**Validates: Requirements 13.1**

### Property 33: DLQ Query Returns Complete Metadata

_For any_ message in Dead_Letter_Queue, querying should return recipient status, failure reason, and retry count.

**Validates: Requirements 13.3**

### Property 34: Manual Retry Requeues Message

_For any_ message manually retried from Dead_Letter_Queue, the message should be returned to Message_Queue with status "Queued".

**Validates: Requirements 13.4**

### Property 35: Manual Retry Resets Retry Count

_For any_ message manually retried from Dead_Letter_Queue, the retry count should be reset to 0.

**Validates: Requirements 13.5**

### Property 36: State Transitions Are Logged

_For any_ message status transition, a log entry should be created with the transition timestamp and correlation identifier.

**Validates: Requirements 14.2**

### Property 37: Backpressure Applied at Queue Threshold

_For any_ message submission when Message_Queue depth exceeds 100,000 messages, the submission should be rejected with backpressure indication.

**Validates: Requirements 15.1**

### Property 38: Rate Limiting Enforced Per User

_For any_ business user, submitting more than 100 messages within a 60-second window should result in rejection of excess messages.

**Validates: Requirements 15.2**

## Error Handling

### Validation Errors

- **Invalid Recipient**: Return 400 Bad Request with error message "Recipient user not found"
- **Empty Content**: Return 400 Bad Request with error message "Message content cannot be empty"
- **Invalid Media Type**: Return 400 Bad Request with error message "Unsupported media type: {type}. Supported types: JPEG, PNG, GIF, PDF, MP4, MOV"
- **File Size Exceeded**: Return 400 Bad Request with error message "File size exceeds 10MB limit"
- **Too Many Attachments**: Return 400 Bad Request with error message "Maximum 5 attachments allowed per message"

### Delivery Errors

- **User Offline**: Queue message with status "Pending", log info message
- **SignalR Connection Lost**: Retry delivery according to Retry_Policy, log warning
- **Acknowledgment Timeout**: Retry delivery, increment retry count, log warning
- **Max Retries Exceeded**: Move to Dead_Letter_Queue, update status to "Failed", log error and notify administrators
- **Lock Acquisition Failed**: Skip processing (another worker has the message), log debug message

### Infrastructure Errors

- **RabbitMQ Unavailable**: Open circuit breaker, queue messages locally, log error
- **Database Unavailable**: Return 503 Service Unavailable, log critical error
- **Redis Cache Unavailable**: Fall back to database queries, log warning
- **Blob Storage Unavailable**: Return 503 Service Unavailable for media uploads, log error

### Rate Limiting and Backpressure

- **Rate Limit Exceeded**: Return 429 Too Many Requests with Retry-After header, log info
- **Queue Depth Exceeded**: Return 503 Service Unavailable with message "System at capacity", log warning
- **Circuit Breaker Open**: Return 503 Service Unavailable with message "Service temporarily unavailable", log warning

## Testing Strategy

### Unit Testing

Unit tests will verify specific examples, edge cases, and error conditions:

- **Message Entity Tests**: Test state transitions, validation logic, retry count increments
- **MessageManager Tests**: Test message creation, lock acquisition/release, DLQ operations
- **OnlineUserTrackerService Tests**: Test connection tracking, heartbeat updates, stale connection cleanup
- **RetryPolicyService Tests**: Test backoff calculation, retry eligibility, max retry enforcement
- **CircuitBreakerService Tests**: Test failure threshold detection, circuit opening/closing
- **MessageAppService Tests**: Test API endpoints, authorization, rate limiting
- **DeliveryHandler Tests**: Test online/offline routing, acknowledgment handling, error scenarios

### Property-Based Testing

Property-based tests will verify universal properties across all inputs using a minimum of 100 iterations per test:

**Testing Framework**: Use **FsCheck** for .NET property-based testing

**Test Organization**: Create test files in `test/Esh3arTech.Domain.Tests/Messages/` and `test/Esh3arTech.Application.Tests/Messages/`

**Key Property Tests**:

1. **Message Uniqueness** (Property 1): Generate random message batches, verify all IDs are unique
2. **Persistence Round-Trip** (Property 2): Generate random messages, save and retrieve, verify equality
3. **Validation Rejection** (Properties 3, 4): Generate invalid messages, verify all are rejected
4. **Status Transitions** (Properties 5-8, 15): Generate random state transitions, verify only valid paths allowed
5. **Retry Logic** (Properties 18-20): Generate failure scenarios, verify backoff intervals and retry counts
6. **Distributed Locking** (Properties 25-26): Simulate concurrent workers, verify no duplicate processing
7. **Idempotency** (Property 28): Generate duplicate delivery attempts, verify single delivery
8. **Rate Limiting** (Property 38): Generate message bursts, verify rate limits enforced

**Test Tagging**: Each property test must include a comment referencing its design property:

```csharp
// Feature: guaranteed-message-delivery, Property 1: Message Creation Returns Unique Identifiers
[Property]
public Property MessageIds_ShouldBeUnique() { ... }
```

### Integration Testing

Integration tests will verify component interactions:

- **SignalR Hub Tests**: Test real WebSocket connections, message delivery, acknowledgments
- **RabbitMQ Integration**: Test event publishing/subscribing, message persistence
- **Redis Cache Integration**: Test connection state caching, distributed locking
- **Database Integration**: Test message persistence, querying, transactions
- **Blob Storage Integration**: Test media upload, URL generation, access

### Load and Performance Testing

Performance tests will verify non-functional requirements:

- **Throughput Test**: Verify 10,000+ messages/second processing capacity
- **Latency Test**: Verify p95 latency under 200ms for online delivery
- **Scalability Test**: Verify horizontal scaling with multiple workers
- **Stress Test**: Verify graceful degradation under extreme load
- **Endurance Test**: Verify system stability over 24+ hours

### Monitoring and Observability

**Metrics to Track**:

- Message throughput (messages/second)
- Delivery latency (p50, p95, p99)
- Queue depth (current, max)
- Error rate (failures/total)
- Retry rate (retries/total)
- Dead letter queue size
- Online user count
- Connection count
- Worker health status

**Logging Strategy**:

- Structured logging with Serilog
- Correlation IDs for request tracing
- Log levels: Debug (development), Information (production), Warning (errors), Error (failures), Critical (system down)
- Log retention: 90 days minimum

**Alerting Thresholds**:

- Error rate > 5%: Warning alert
- Error rate > 10%: Critical alert
- Queue depth > 50,000: Warning alert
- Queue depth > 100,000: Critical alert
- Dead letter queue > 100: Warning alert
- Worker down: Critical alert
- Database connection failures: Critical alert
