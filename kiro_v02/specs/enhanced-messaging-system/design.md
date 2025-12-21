# Design Document: Enhanced Messaging System

## Overview

This design document outlines the technical architecture for enhancing the Esh3arTech messaging system to support dual-mode messaging (one-way and chat), media sharing, push notifications, reliable delivery, and horizontal scalability. The design maintains backward compatibility with existing one-way messaging while adding comprehensive chat capabilities.

### Design Goals

1. Support both one-way read-only messages and bidirectional chat conversations
2. Enable media sharing (images, videos, audio, documents) in both modes
3. Provide reliable message delivery with retry mechanisms and guaranteed delivery
4. Achieve horizontal scalability to support 100K+ concurrent users
5. Maintain low latency (<200ms) and high throughput (10K+ messages/second)
6. Ensure high availability (99.9% uptime) with fault tolerance
7. Preserve existing ABP Framework DDD architecture and patterns

### Key Architectural Decisions

- **Dual-Mode Design**: Single Message entity with MessageType discriminator
- **Async Processing**: RabbitMQ for message queuing and async delivery
- **Distributed State**: Redis for SignalR backplane and connection tracking
- **Media Storage**: Azure Blob Storage (or AWS S3) for file storage
- **Push Notifications**: Firebase Cloud Messaging (FCM) and Apple Push Notification Service (APNS)
- **Caching Strategy**: Redis for frequently accessed data (conversations, recent messages)
- **Database**: SQL Server with read replicas for query scaling

## Architecture

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                          Load Balancer                               │
└────────────────────────────┬────────────────────────────────────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
┌───────▼────────┐  ┌────────▼───────┐  ┌────────▼───────┐
│  Web Instance  │  │  Web Instance  │  │  Web Instance  │
│   (ASP.NET)    │  │   (ASP.NET)    │  │   (ASP.NET)    │
│  + SignalR Hub │  │  + SignalR Hub │  │  + SignalR Hub │
└───────┬────────┘  └────────┬───────┘  └────────┬───────┘
        │                    │                    │
        └────────────────────┼────────────────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
        ▼                    ▼                    ▼
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│    Redis     │    │   RabbitMQ   │    │  SQL Server  │
│  (Backplane  │    │   (Message   │    │  (Primary +  │
│  + Cache)    │    │    Queue)    │    │   Replicas)  │
└──────────────┘    └──────┬───────┘    └──────────────┘
                           │
                    ┌──────▼───────┐
                    │   Message    │
                    │  Processing  │
                    │   Workers    │
                    └──────┬───────┘
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
        ▼                  ▼                  ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ Blob Storage │  │     FCM      │  │     APNS     │
│   (Media)    │  │   (Android)  │  │    (iOS)     │
└──────────────┘  └──────────────┘  └──────────────┘
```


### Component Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      Presentation Layer                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │ Razor Pages  │  │  SignalR Hub │  │  API         │          │
│  │ (Web UI)     │  │  (Real-time) │  │  Controllers │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
└─────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────────┐
│                      Application Layer                           │
│  ┌──────────────────┐  ┌──────────────────┐                    │
│  │ MessageAppService│  │ConversationAppSvc│                    │
│  │ - SendOneWay     │  │ - CreateChat     │                    │
│  │ - SendChat       │  │ - GetHistory     │                    │
│  │ - GetPending     │  │ - ListConvos     │                    │
│  └──────────────────┘  └──────────────────┘                    │
│  ┌──────────────────┐  ┌──────────────────┐                    │
│  │MediaAppService   │  │DeviceTokenAppSvc │                    │
│  │ - UploadMedia    │  │ - Register       │                    │
│  │ - GetMediaUrl    │  │ - Unregister     │                    │
│  └──────────────────┘  └──────────────────┘                    │
└─────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────────┐
│                        Domain Layer                              │
│  ┌──────────────────┐  ┌──────────────────┐                    │
│  │ Message (Entity) │  │Conversation      │                    │
│  │ - MessageType    │  │ (Entity)         │                    │
│  │ - Status         │  │ - Participants   │                    │
│  │ - Content        │  │ - LastMessage    │                    │
│  └──────────────────┘  └──────────────────┘                    │
│  ┌──────────────────┐  ┌──────────────────┐                    │
│  │MediaAttachment   │  │DeviceToken       │                    │
│  │ (Entity)         │  │ (Entity)         │                    │
│  └──────────────────┘  └──────────────────┘                    │
│  ┌──────────────────┐  ┌──────────────────┐                    │
│  │MessageManager    │  │ConversationMgr   │                    │
│  │ (Domain Service) │  │ (Domain Service) │                    │
│  └──────────────────┘  └──────────────────┘                    │
└─────────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                          │
│  ┌──────────────────┐  ┌──────────────────┐                    │
│  │ EF Core DbContext│  │ Redis Cache      │                    │
│  │ - Repositories   │  │ - Distributed    │                    │
│  └──────────────────┘  └──────────────────┘                    │
│  ┌──────────────────┐  ┌──────────────────┐                    │
│  │ RabbitMQ         │  │ Blob Storage     │                    │
│  │ - Event Bus      │  │ - Media Files    │                    │
│  └──────────────────┘  └──────────────────┘                    │
│  ┌──────────────────┐  ┌──────────────────┐                    │
│  │ FCM Client       │  │ APNS Client      │                    │
│  │ - Push Notifs    │  │ - Push Notifs    │                    │
│  └──────────────────┘  └──────────────────┘                    │
└─────────────────────────────────────────────────────────────────┘
```


## Components and Interfaces

### 1. Message Domain Model

#### Message Entity (Enhanced)

```csharp
public class Message : FullAuditedAggregateRoot<Guid>
{
    // Existing fields
    public string RecipientPhoneNumber { get; private set; }
    public string Subject { get; private set; }  // Only for OneWay messages
    public MessageContentType ContentType { get; private set; }
    public string MessageContent { get; private set; }
    public MessageStatus Status { get; private set; }
    
    // New fields for dual-mode support
    public MessageType MessageType { get; private set; }  // OneWay or Chat
    public Guid? ConversationId { get; private set; }  // Null for OneWay
    public string? SenderPhoneNumber { get; private set; }  // For Chat messages
    public int RetryCount { get; private set; }  // For reliability
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public string? FailureReason { get; private set; }
    public int Priority { get; private set; }  // 0=Low, 1=Normal, 2=High
    
    // Navigation properties
    public virtual ICollection<MediaAttachment> MediaAttachments { get; set; }
    public virtual Conversation? Conversation { get; set; }
}
```

#### Conversation Entity (New)

```csharp
public class Conversation : FullAuditedAggregateRoot<Guid>
{
    public Guid BusinessUserId { get; private set; }  // IdentityUser
    public string MobileUserPhoneNumber { get; private set; }
    public Guid? LastMessageId { get; private set; }
    public DateTime? LastMessageAt { get; private set; }
    public int UnreadCountBusiness { get; private set; }
    public int UnreadCountMobile { get; private set; }
    public ConversationStatus Status { get; private set; }  // Active, Archived, Closed
    
    // Navigation properties
    public virtual ICollection<Message> Messages { get; set; }
    public virtual IdentityUser BusinessUser { get; set; }
    public virtual MobileUser MobileUser { get; set; }
    
    // Domain methods
    public void AddMessage(Message message);
    public void MarkAsRead(bool isByBusinessUser);
    public void Archive();
    public void Close();
}
```

#### MediaAttachment Entity (New)

```csharp
public class MediaAttachment : Entity<Guid>
{
    public Guid MessageId { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }  // MIME type
    public long FileSizeBytes { get; private set; }
    public string BlobStorageUrl { get; private set; }
    public string? ThumbnailSmallUrl { get; private set; }  // 150x150
    public string? ThumbnailMediumUrl { get; private set; }  // 300x300
    public string? ThumbnailLargeUrl { get; private set; }  // 600x600
    public MediaType MediaType { get; private set; }  // Image, Video, Audio, Document
    public DateTime CreatedAt { get; private set; }
    
    // Navigation
    public virtual Message Message { get; set; }
}
```

#### DeviceToken Entity (New)

```csharp
public class DeviceToken : Entity<Guid>
{
    public string MobileUserPhoneNumber { get; private set; }
    public string Token { get; private set; }
    public DevicePlatform Platform { get; private set; }  // Android, iOS
    public bool IsActive { get; private set; }
    public int FailureCount { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    // Navigation
    public virtual MobileUser MobileUser { get; set; }
    
    // Domain methods
    public void MarkAsUsed();
    public void IncrementFailureCount();
    public void Deactivate();
}
```


### 2. Enumerations

```csharp
// New enum
public enum MessageType : byte
{
    OneWay = 0,  // Read-only from business to mobile
    Chat = 1     // Bidirectional conversation
}

// Enhanced enum
public enum MessageStatus : byte
{
    Pending = 0,    // Created, not yet sent
    Sent = 1,       // Enqueued for delivery
    Delivered = 2,  // Received by recipient device
    Read = 3,       // Opened by recipient
    Failed = 4      // Delivery failed after retries
}

// New enum
public enum ConversationStatus : byte
{
    Active = 0,
    Archived = 1,
    Closed = 2
}

// New enum
public enum MediaType : byte
{
    Image = 0,
    Video = 1,
    Audio = 2,
    Document = 3
}

// New enum
public enum DevicePlatform : byte
{
    Android = 0,
    iOS = 1
}
```

### 3. Domain Services

#### MessageManager (Enhanced)

```csharp
public class MessageManager : DomainService
{
    private readonly UserPlanManager _userPlanManager;
    private readonly IRepository<Conversation, Guid> _conversationRepository;
    
    // Create one-way message (existing + enhanced)
    public async Task<Message> CreateOneWayMessageAsync(
        Guid businessUserId,
        string recipientPhoneNumber,
        string subject,
        string messageContent,
        int priority = 1);
    
    // Create chat message (new)
    public async Task<Message> CreateChatMessageAsync(
        Guid conversationId,
        string senderPhoneNumber,
        string messageContent,
        bool isByBusinessUser);
    
    // Update message status
    public void UpdateStatus(Message message, MessageStatus newStatus);
    
    // Increment retry count
    public void IncrementRetry(Message message);
    
    // Mark as failed
    public void MarkAsFailed(Message message, string reason);
}
```

#### ConversationManager (New)

```csharp
public class ConversationManager : DomainService
{
    private readonly IRepository<Conversation, Guid> _conversationRepository;
    private readonly IRepository<MobileUser, Guid> _mobileUserRepository;
    
    // Create or get existing conversation
    public async Task<Conversation> GetOrCreateConversationAsync(
        Guid businessUserId,
        string mobileUserPhoneNumber);
    
    // Add message to conversation
    public async Task AddMessageToConversationAsync(
        Conversation conversation,
        Message message);
    
    // Mark messages as read
    public async Task MarkAsReadAsync(
        Conversation conversation,
        bool isByBusinessUser);
    
    // Get unread count
    public int GetUnreadCount(Conversation conversation, bool forBusinessUser);
}
```


### 4. Application Services

#### IMessageAppService (Enhanced)

```csharp
public interface IMessageAppService : IApplicationService
{
    // One-way messaging (existing + enhanced)
    Task<MessageDto> SendOneWayMessageAsync(SendOneWayMessageDto input);
    Task<PagedResultDto<MessageInListDto>> GetOneWayMessagesAsync(
        GetMessagesInput input);
    Task<IReadOnlyList<PendingMessageDto>> GetPendingMessagesAsync(
        string phoneNumber);
    
    // Chat messaging (new)
    Task<MessageDto> SendChatMessageAsync(SendChatMessageDto input);
    Task<PagedResultDto<MessageInListDto>> GetConversationMessagesAsync(
        Guid conversationId, 
        GetMessagesInput input);
    
    // Status updates
    Task UpdateMessageStatusAsync(Guid messageId, MessageStatus status);
    Task MarkMessageAsReadAsync(Guid messageId);
    
    // Broadcast (existing)
    Task BroadcastAsync(BroadcastMessageDto input);
}
```

#### IConversationAppService (New)

```csharp
public interface IConversationAppService : IApplicationService
{
    // Conversation management
    Task<ConversationDto> GetOrCreateConversationAsync(
        CreateConversationDto input);
    Task<ConversationDto> GetConversationAsync(Guid id);
    Task<PagedResultDto<ConversationInListDto>> GetConversationsAsync(
        GetConversationsInput input);
    
    // Conversation actions
    Task MarkConversationAsReadAsync(Guid conversationId);
    Task ArchiveConversationAsync(Guid conversationId);
    Task CloseConversationAsync(Guid conversationId);
    
    // Statistics
    Task<int> GetUnreadConversationCountAsync();
}
```

#### IMediaAppService (New)

```csharp
public interface IMediaAppService : IApplicationService
{
    // Upload media
    Task<MediaAttachmentDto> UploadMediaAsync(UploadMediaDto input);
    
    // Get media URL (time-limited signed URL)
    Task<string> GetMediaUrlAsync(Guid mediaAttachmentId);
    
    // Get thumbnail URL
    Task<string> GetThumbnailUrlAsync(
        Guid mediaAttachmentId, 
        ThumbnailSize size);
    
    // Delete media
    Task DeleteMediaAsync(Guid mediaAttachmentId);
}
```

#### IDeviceTokenAppService (New)

```csharp
public interface IDeviceTokenAppService : IApplicationService
{
    // Register device for push notifications
    Task RegisterDeviceAsync(RegisterDeviceDto input);
    
    // Unregister device
    Task UnregisterDeviceAsync(string token);
    
    // Update device token
    Task UpdateDeviceTokenAsync(UpdateDeviceTokenDto input);
    
    // Get user devices
    Task<List<DeviceTokenDto>> GetUserDevicesAsync();
}
```


## Data Models

### Database Schema

#### Messages Table (Enhanced)

```sql
CREATE TABLE Messages (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    MessageType TINYINT NOT NULL,  -- 0=OneWay, 1=Chat
    ConversationId UNIQUEIDENTIFIER NULL,  -- FK to Conversations
    RecipientPhoneNumber NVARCHAR(20) NOT NULL,
    SenderPhoneNumber NVARCHAR(20) NULL,  -- For Chat messages
    Subject NVARCHAR(200) NULL,  -- Only for OneWay
    MessageContent NVARCHAR(MAX) NOT NULL,
    ContentType TINYINT NOT NULL,  -- 0=Json, 1=Text
    Status TINYINT NOT NULL,  -- 0=Pending, 1=Sent, 2=Delivered, 3=Read, 4=Failed
    Priority INT NOT NULL DEFAULT 1,
    RetryCount INT NOT NULL DEFAULT 0,
    FailureReason NVARCHAR(500) NULL,
    DeliveredAt DATETIME2 NULL,
    ReadAt DATETIME2 NULL,
    CreatorId UNIQUEIDENTIFIER NOT NULL,  -- FK to IdentityUsers
    CreationTime DATETIME2 NOT NULL,
    LastModificationTime DATETIME2 NULL,
    DeleterId UNIQUEIDENTIFIER NULL,
    DeletionTime DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    INDEX IX_Messages_ConversationId (ConversationId),
    INDEX IX_Messages_RecipientPhoneNumber (RecipientPhoneNumber),
    INDEX IX_Messages_Status (Status),
    INDEX IX_Messages_MessageType (MessageType),
    INDEX IX_Messages_CreationTime (CreationTime DESC)
);
```

#### Conversations Table (New)

```sql
CREATE TABLE Conversations (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    BusinessUserId UNIQUEIDENTIFIER NOT NULL,  -- FK to IdentityUsers
    MobileUserPhoneNumber NVARCHAR(20) NOT NULL,
    LastMessageId UNIQUEIDENTIFIER NULL,  -- FK to Messages
    LastMessageAt DATETIME2 NULL,
    UnreadCountBusiness INT NOT NULL DEFAULT 0,
    UnreadCountMobile INT NOT NULL DEFAULT 0,
    Status TINYINT NOT NULL DEFAULT 0,  -- 0=Active, 1=Archived, 2=Closed
    CreatorId UNIQUEIDENTIFIER NULL,
    CreationTime DATETIME2 NOT NULL,
    LastModificationTime DATETIME2 NULL,
    DeleterId UNIQUEIDENTIFIER NULL,
    DeletionTime DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    UNIQUE INDEX UX_Conversations_Participants (BusinessUserId, MobileUserPhoneNumber),
    INDEX IX_Conversations_BusinessUserId (BusinessUserId),
    INDEX IX_Conversations_MobileUserPhoneNumber (MobileUserPhoneNumber),
    INDEX IX_Conversations_LastMessageAt (LastMessageAt DESC),
    INDEX IX_Conversations_Status (Status)
);
```

#### MediaAttachments Table (New)

```sql
CREATE TABLE MediaAttachments (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    MessageId UNIQUEIDENTIFIER NOT NULL,  -- FK to Messages
    FileName NVARCHAR(255) NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    FileSizeBytes BIGINT NOT NULL,
    BlobStorageUrl NVARCHAR(500) NOT NULL,
    ThumbnailSmallUrl NVARCHAR(500) NULL,
    ThumbnailMediumUrl NVARCHAR(500) NULL,
    ThumbnailLargeUrl NVARCHAR(500) NULL,
    MediaType TINYINT NOT NULL,  -- 0=Image, 1=Video, 2=Audio, 3=Document
    CreatedAt DATETIME2 NOT NULL,
    
    INDEX IX_MediaAttachments_MessageId (MessageId)
);
```

#### DeviceTokens Table (New)

```sql
CREATE TABLE DeviceTokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    MobileUserPhoneNumber NVARCHAR(20) NOT NULL,
    Token NVARCHAR(500) NOT NULL,
    Platform TINYINT NOT NULL,  -- 0=Android, 1=iOS
    IsActive BIT NOT NULL DEFAULT 1,
    FailureCount INT NOT NULL DEFAULT 0,
    LastUsedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL,
    
    UNIQUE INDEX UX_DeviceTokens_Token (Token),
    INDEX IX_DeviceTokens_MobileUserPhoneNumber (MobileUserPhoneNumber),
    INDEX IX_DeviceTokens_IsActive (IsActive)
);
```


### Redis Cache Schema

#### Connection Tracking

```
Key: "signalr:connections:{phoneNumber}"
Type: Set
Value: ["connectionId1", "connectionId2", ...]
TTL: 5 minutes (auto-refresh on activity)
```

#### Recent Messages Cache

```
Key: "messages:conversation:{conversationId}:recent"
Type: List
Value: [MessageDto JSON, ...]
Size: Last 50 messages
TTL: 5 minutes
```

#### Conversation List Cache

```
Key: "conversations:user:{userId}:list"
Type: Sorted Set
Score: LastMessageAt timestamp
Value: ConversationDto JSON
TTL: 5 minutes
```

#### Unread Count Cache

```
Key: "conversations:{conversationId}:unread:{userType}"
Type: String (integer)
Value: Unread count
TTL: 5 minutes
```


## Message Flow Diagrams

### One-Way Message Flow

```
Business User                 API                Queue              Worker              Mobile User
     |                         |                   |                  |                      |
     |--SendOneWayMessage----->|                   |                  |                      |
     |                         |                   |                  |                      |
     |                         |--Validate-------->|                  |                      |
     |                         |--CreateMessage--->|                  |                      |
     |                         |--EnqueueMessage-->|                  |                      |
     |<----MessageId-----------|                   |                  |                      |
     |                         |                   |                  |                      |
     |                         |                   |--DequeueMessage->|                      |
     |                         |                   |                  |--CheckOnline-------->|
     |                         |                   |                  |                      |
     |                         |                   |                  |  IF ONLINE:          |
     |                         |                   |                  |--SendViaSignalR----->|
     |                         |                   |                  |<---Acknowledge-------|
     |                         |                   |                  |--UpdateStatus------->|
     |                         |                   |                  |                      |
     |                         |                   |                  |  IF OFFLINE:         |
     |                         |                   |                  |--SendPushNotif------>|
     |                         |                   |                  |--MarkAsPending------>|
     |                         |                   |                  |                      |
     |                         |                   |                  |  (User comes online) |
     |                         |                   |                  |<---Connect-----------|
     |                         |                   |                  |--GetPending--------->|
     |                         |                   |                  |--SendPending-------->|
     |                         |                   |                  |<---Acknowledge-------|
```

### Chat Message Flow

```
Mobile User              SignalR Hub           Queue              Worker           Business User
     |                         |                   |                  |                   |
     |--SendChatMessage------->|                   |                  |                   |
     |                         |--Validate-------->|                  |                   |
     |                         |--GetConversation->|                  |                   |
     |                         |--CreateMessage--->|                  |                   |
     |                         |--EnqueueMessage-->|                  |                   |
     |<----Acknowledge---------|                   |                  |                   |
     |                         |                   |                  |                   |
     |                         |                   |--DequeueMessage->|                   |
     |                         |                   |                  |--CheckOnline----->|
     |                         |                   |                  |                   |
     |                         |                   |                  |  IF ONLINE:       |
     |                         |                   |                  |--SendViaSignalR-->|
     |                         |                   |                  |<--Acknowledge-----|
     |                         |                   |                  |--UpdateStatus---->|
     |                         |                   |                  |                   |
     |                         |                   |                  |  IF OFFLINE:      |
     |                         |                   |                  |--SendPushNotif--->|
     |                         |                   |                  |--MarkAsPending--->|
```


### Retry Mechanism Flow

```
Message Delivery Attempt
         |
         v
    Is Online?
    /        \
  YES        NO
   |          |
   v          v
Send via   Send Push
SignalR    Notification
   |          |
   v          v
Success?   Success?
/    \     /    \
YES  NO   YES  NO
 |    |    |    |
 v    v    v    v
Done Retry Done Retry
         |         |
         v         v
    Wait (exponential backoff)
    1s, 2s, 4s, 8s, 16s, 32s
         |
         v
    Retry < 6?
    /        \
  YES        NO
   |          |
   v          v
Retry     Move to DLQ
Again     + Notify Admin
```


## Error Handling

### Retry Strategy

**Exponential Backoff Configuration:**
- Initial delay: 1 second
- Backoff multiplier: 2x
- Maximum attempts: 6
- Total max wait time: 63 seconds (1+2+4+8+16+32)

**Retry Scenarios:**
1. SignalR connection lost
2. Push notification service unavailable
3. Database deadlock/timeout
4. Blob storage upload failure
5. Redis cache unavailable

**Non-Retryable Errors:**
- Invalid phone number format
- User not found
- Permission denied
- Message content validation failure

### Circuit Breaker Pattern

Using Polly library for circuit breaker implementation:

```csharp
// Push Notification Circuit Breaker
services.AddHttpClient<IPushNotificationService, PushNotificationService>()
    .AddPolicyHandler(Policy
        .Handle<HttpRequestException>()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromMinutes(1)
        ));

// Blob Storage Circuit Breaker
services.AddHttpClient<IBlobStorageService, BlobStorageService>()
    .AddPolicyHandler(Policy
        .Handle<Exception>()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromSeconds(30)
        ));
```

### Dead Letter Queue

Messages that fail after maximum retry attempts are moved to a dead letter queue:

**DLQ Structure:**
- Queue Name: `messaging.deadletter`
- Message Format: Original message + failure metadata
- Retention: 7 days
- Admin notification: Email alert on DLQ entry

**DLQ Processing:**
- Manual review via admin dashboard
- Ability to requeue messages
- Bulk operations for common failure patterns


## Testing Strategy

### Unit Testing

**Test Coverage Requirements:**
- Domain entities: 90%+ coverage
- Domain services: 85%+ coverage
- Application services: 80%+ coverage

**Key Unit Tests:**
- Message entity validation
- Conversation state transitions
- MessageManager business logic
- ConversationManager operations
- Status update workflows

**Testing Framework:**
- xUnit for test framework
- Shouldly for assertions
- NSubstitute for mocking

### Property-Based Testing

Property-based tests will be implemented using **FsCheck** library to verify universal properties across all valid inputs.

**Testing Approach:**
- Configure each property test to run minimum 100 iterations
- Tag each test with the correctness property it validates
- Use format: `**Feature: enhanced-messaging-system, Property {number}: {property_text}**`

### Integration Testing

**Test Scenarios:**
- End-to-end message delivery (one-way and chat)
- SignalR hub connectivity and message routing
- RabbitMQ message queuing and processing
- Redis caching and backplane
- Blob storage upload and retrieval
- Push notification delivery

**Test Environment:**
- In-memory database for fast tests
- TestContainers for Redis and RabbitMQ
- Mock push notification services


## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Message Type Consistency
*For any* message, if the message type is OneWay, then the conversation ID must be null, and if the message type is Chat, then the conversation ID must not be null.
**Validates: Requirements 1.1, 1.12**

### Property 2: Conversation Participant Uniqueness
*For any* business user and mobile user pair, there should exist at most one active conversation between them.
**Validates: Requirements 1.11**

### Property 3: Message Status Progression
*For any* message, status transitions must follow valid progressions: Pending → Sent → Delivered → Read, or Pending → Sent → Failed. Invalid transitions (e.g., Read → Pending) should be rejected.
**Validates: Requirements 9.1, 9.2, 9.3, 9.4, 9.5**

### Property 4: Retry Count Monotonicity
*For any* message with retry attempts, the retry count must be monotonically increasing and never exceed the maximum retry limit of 6.
**Validates: Requirements 4.1, 4.2**

### Property 5: Media File Size Validation
*For any* media upload, if the media type is Video, the file size must not exceed 50MB; if Image, must not exceed 10MB; if Document, must not exceed 5MB.
**Validates: Requirements 2.2**

### Property 6: Conversation Unread Count Accuracy
*For any* conversation, the sum of messages with status Delivered or Sent (not Read) for a participant must equal the unread count for that participant.
**Validates: Requirements 1.14**

### Property 7: Device Token Uniqueness
*For any* device token, it must be unique across all users and platforms in the system.
**Validates: Requirements 3.1**

### Property 8: Message Delivery Idempotency
*For any* message, processing the same message ID multiple times should result in the same final state as processing it once.
**Validates: Requirements 4.3, 10.3**

### Property 9: Priority Queue Ordering
*For any* set of messages in the queue, messages with higher priority values must be processed before messages with lower priority values when both are available.
**Validates: Requirements 5.2**

### Property 10: Cache-Database Consistency
*For any* cached conversation or message data, after a cache invalidation and refresh, the cached data must match the database data.
**Validates: Requirements 10.4**

### Property 11: Signed URL Expiration
*For any* media file, the generated signed URL must expire after exactly 1 hour from generation time.
**Validates: Requirements 2.6**

### Property 12: One-Way Message Read-Only
*For any* message with type OneWay, mobile users must not be able to create a reply message in the same context.
**Validates: Requirements 1.4**

### Property 13: Chat Message Bidirectionality
*For any* conversation, both the business user and mobile user must be able to send messages within that conversation.
**Validates: Requirements 1.5, 1.6**

### Property 14: Offline Message Queuing
*For any* message sent to an offline user, the message must be stored with status Pending and delivered when the user comes online.
**Validates: Requirements 1.3, 1.15**

### Property 15: Push Notification Delivery
*For any* message sent to an offline user with registered devices, a push notification must be sent to all active devices.
**Validates: Requirements 3.2, 3.4**


## Infrastructure Configuration

### Redis Configuration

```csharp
// Startup configuration
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration["Redis:ConnectionString"];
    options.InstanceName = "Esh3arTech:";
});

// SignalR backplane
services.AddSignalR()
    .AddStackExchangeRedis(configuration["Redis:ConnectionString"], options =>
    {
        options.Configuration.ChannelPrefix = "Esh3arTech.SignalR";
    });

// Distributed cache for connection tracking
services.AddSingleton<IDistributedConnectionTracker, RedisConnectionTracker>();
```

### RabbitMQ Configuration

```csharp
// Message queue configuration
Configure<AbpRabbitMqEventBusOptions>(options =>
{
    options.ClientName = "Esh3arTech.Messaging";
    options.ExchangeName = "Esh3arTech.Messaging";
    options.PrefetchCount = 100;  // Process 100 messages at a time
    
    // Queue configuration
    options.QueueName = "messaging.queue";
    options.DeadLetterQueueName = "messaging.deadletter";
    
    // Retry configuration
    options.RetryCount = 6;
    options.RetryDelaySeconds = new[] { 1, 2, 4, 8, 16, 32 };
});
```

### Blob Storage Configuration

```csharp
// Azure Blob Storage
Configure<AbpBlobStoringOptions>(options =>
{
    options.Containers.Configure<MediaContainer>(container =>
    {
        container.UseAzure(azure =>
        {
            azure.ConnectionString = configuration["Azure:BlobStorage:ConnectionString"];
            azure.ContainerName = "media-attachments";
            azure.CreateContainerIfNotExists = true;
        });
    });
});

// Alternative: AWS S3
Configure<AbpBlobStoringOptions>(options =>
{
    options.Containers.Configure<MediaContainer>(container =>
    {
        container.UseAws(aws =>
        {
            aws.AccessKeyId = configuration["AWS:AccessKeyId"];
            aws.SecretAccessKey = configuration["AWS:SecretAccessKey"];
            aws.Region = configuration["AWS:Region"];
            aws.BucketName = "esh3artech-media";
            aws.CreateBucketIfNotExists = true;
        });
    });
});
```

### Push Notification Configuration

```csharp
// Firebase Cloud Messaging (Android)
services.AddSingleton<IFcmService>(sp =>
{
    var credential = GoogleCredential.FromFile(
        configuration["Firebase:CredentialPath"]);
    return new FcmService(credential);
});

// Apple Push Notification Service (iOS)
services.AddSingleton<IApnsService>(sp =>
{
    var options = new ApnsOptions
    {
        CertificatePath = configuration["APNS:CertificatePath"],
        CertificatePassword = configuration["APNS:CertificatePassword"],
        IsProduction = !hostEnvironment.IsDevelopment()
    };
    return new ApnsService(options);
});
```


### Health Checks Configuration

```csharp
services.AddHealthChecks()
    .AddSqlServer(
        configuration.GetConnectionString("Default"),
        name: "database",
        tags: new[] { "db", "sql" })
    .AddRedis(
        configuration["Redis:ConnectionString"],
        name: "redis",
        tags: new[] { "cache", "redis" })
    .AddRabbitMQ(
        configuration["RabbitMQ:ConnectionString"],
        name: "rabbitmq",
        tags: new[] { "queue", "rabbitmq" })
    .AddAzureBlobStorage(
        configuration["Azure:BlobStorage:ConnectionString"],
        name: "blob-storage",
        tags: new[] { "storage", "blob" })
    .AddCheck<SignalRHealthCheck>("signalr", tags: new[] { "realtime" });

// Health check UI
services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(30);
    setup.MaximumHistoryEntriesPerEndpoint(50);
    setup.AddHealthCheckEndpoint("Esh3arTech API", "/health");
})
.AddInMemoryStorage();
```

### Load Balancer Configuration

**Nginx Configuration Example:**

```nginx
upstream esh3artech_backend {
    least_conn;  # Load balancing method
    server web1.esh3artech.com:5000 max_fails=3 fail_timeout=30s;
    server web2.esh3artech.com:5000 max_fails=3 fail_timeout=30s;
    server web3.esh3artech.com:5000 max_fails=3 fail_timeout=30s;
}

server {
    listen 80;
    server_name esh3artech.com;
    
    location / {
        proxy_pass http://esh3artech_backend;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # SignalR specific
        proxy_cache_bypass $http_upgrade;
        proxy_read_timeout 300s;
        proxy_connect_timeout 75s;
    }
    
    location /health {
        proxy_pass http://esh3artech_backend/health;
        access_log off;
    }
}
```


## Performance Optimization

### Database Optimization

**Indexes:**
```sql
-- Message queries
CREATE INDEX IX_Messages_ConversationId_CreationTime 
    ON Messages(ConversationId, CreationTime DESC) 
    INCLUDE (MessageContent, Status);

CREATE INDEX IX_Messages_RecipientPhone_Status 
    ON Messages(RecipientPhoneNumber, Status) 
    WHERE Status IN (0, 1);  -- Pending, Sent

-- Conversation queries
CREATE INDEX IX_Conversations_BusinessUser_LastMessage 
    ON Conversations(BusinessUserId, LastMessageAt DESC) 
    WHERE Status = 0;  -- Active only

CREATE INDEX IX_Conversations_MobileUser_LastMessage 
    ON Conversations(MobileUserPhoneNumber, LastMessageAt DESC) 
    WHERE Status = 0;
```

**Query Optimization:**
- Use `AsNoTracking()` for read-only queries
- Implement pagination with cursor-based navigation
- Use projection to select only needed columns
- Batch operations for bulk inserts/updates

**Read Replicas:**
- Configure read replicas for query scaling
- Route read operations to replicas
- Route write operations to primary

### Caching Strategy

**Cache Hierarchy:**
1. **L1 Cache (In-Memory)**: Frequently accessed static data (5 min TTL)
2. **L2 Cache (Redis)**: Distributed cache for dynamic data (5 min TTL)
3. **Database**: Source of truth

**Cache Invalidation:**
- Write-through: Update cache on write
- Time-based: TTL expiration
- Event-based: Invalidate on domain events

**Cached Data:**
- Recent messages per conversation (last 50)
- Conversation list per user
- Unread counts
- User connection mappings
- Media URLs (signed URLs cached for 50 minutes)

### Message Queue Optimization

**Partitioning Strategy:**
- Partition by conversation ID for chat messages
- Partition by recipient phone for one-way messages
- Enables parallel processing across workers

**Batch Processing:**
- Process up to 100 messages per batch
- Reduces database round trips
- Improves throughput

**Priority Queues:**
- High priority: 0-2 (urgent messages)
- Normal priority: 3-5 (regular messages)
- Low priority: 6-10 (bulk/broadcast messages)


## Security Considerations

### Authentication & Authorization

**Message Sending Permissions:**
- One-way messages: `Esh3arTechPermissions.Esh3arSendMessages`
- Chat messages: `Esh3arTechPermissions.Esh3arChat`
- Broadcast messages: `Esh3arTechPermissions.Esh3arBroadcast`

**Data Access Control:**
- Users can only access their own conversations
- Business users can only access conversations they participate in
- Mobile users can only access their own messages and conversations

### Data Protection

**Encryption:**
- TLS 1.2+ for all network communication
- Encrypt sensitive data at rest (connection strings, API keys)
- Use Azure Key Vault or AWS Secrets Manager for secrets

**PII Handling:**
- Phone numbers are considered PII
- Implement data retention policies
- Support GDPR right to deletion
- Audit log all PII access

### Rate Limiting

**Per-User Limits:**
- 100 messages per minute per user
- 1000 messages per hour per user
- 10 media uploads per minute per user

**Global Limits:**
- 10,000 messages per second system-wide
- Queue depth limit: 100,000 messages

**Implementation:**
```csharp
services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("messaging", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueLimit = 10;
    });
});
```

### Input Validation

**Message Content:**
- Max length: 10,000 characters
- Sanitize HTML/script tags
- Validate phone number format
- Check for malicious content

**Media Files:**
- Validate file extensions
- Scan for malware (integrate with antivirus service)
- Validate MIME types
- Check file size limits


## Monitoring and Observability

### Metrics to Track

**Business Metrics:**
- Messages sent per minute/hour/day
- Message delivery success rate
- Average delivery time
- Conversation creation rate
- Active conversations count
- Media upload volume

**Technical Metrics:**
- Queue depth and processing rate
- Cache hit/miss ratio
- Database query performance
- SignalR connection count
- API response times (p50, p95, p99)
- Error rates by type

**Infrastructure Metrics:**
- CPU and memory usage
- Network throughput
- Disk I/O
- Redis memory usage
- RabbitMQ queue depth

### Logging Strategy

**Log Levels:**
- **Error**: Failed operations, exceptions
- **Warning**: Retry attempts, degraded performance
- **Information**: Message sent/delivered, conversation created
- **Debug**: Detailed flow information (dev only)

**Structured Logging with Serilog:**
```csharp
Log.Information(
    "Message {MessageId} of type {MessageType} sent to {Recipient} with status {Status}",
    messageId, messageType, recipientPhone, status);

Log.Warning(
    "Message {MessageId} delivery failed, retry attempt {RetryCount}/{MaxRetries}",
    messageId, retryCount, maxRetries);

Log.Error(
    exception,
    "Failed to process message {MessageId} after {RetryCount} attempts",
    messageId, retryCount);
```

### Alerting Rules

**Critical Alerts (Page immediately):**
- Database connection failure
- Redis unavailable
- RabbitMQ unavailable
- Message delivery success rate < 95%
- API error rate > 5%

**Warning Alerts (Email/Slack):**
- Queue depth > 10,000 messages
- Cache hit rate < 80%
- Average delivery time > 5 seconds
- Dead letter queue has messages

**Monitoring Tools:**
- Application Insights (Azure) or CloudWatch (AWS)
- Grafana for dashboards
- PagerDuty or Opsgenie for alerting


## Deployment Architecture

### Multi-Instance Deployment

```
                    ┌─────────────────┐
                    │  Load Balancer  │
                    │   (Nginx/ALB)   │
                    └────────┬────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
┌───────▼────────┐  ┌────────▼───────┐  ┌────────▼───────┐
│ Web Instance 1 │  │ Web Instance 2 │  │ Web Instance 3 │
│  + SignalR     │  │  + SignalR     │  │  + SignalR     │
│  + API         │  │  + API         │  │  + API         │
└───────┬────────┘  └────────┬───────┘  └────────┬───────┘
        │                    │                    │
        └────────────────────┼────────────────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
┌───────▼────────┐  ┌────────▼───────┐  ┌────────▼───────┐
│ Worker Pool 1  │  │ Worker Pool 2  │  │ Worker Pool 3  │
│ (Message Proc) │  │ (Message Proc) │  │ (Message Proc) │
└───────┬────────┘  └────────┬───────┘  └────────┬───────┘
        │                    │                    │
        └────────────────────┼────────────────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
        ▼                    ▼                    ▼
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│ Redis Cluster│    │   RabbitMQ   │    │  SQL Server  │
│ (3 nodes)    │    │   Cluster    │    │  (Primary +  │
│              │    │   (3 nodes)  │    │   2 Replicas)│
└──────────────┘    └──────────────┘    └──────────────┘
```

### Scaling Strategy

**Horizontal Scaling:**
- Add more web instances behind load balancer
- Add more worker instances for message processing
- Scale Redis cluster for cache capacity
- Scale RabbitMQ cluster for queue throughput

**Vertical Scaling:**
- Increase database server resources
- Increase Redis memory
- Increase worker instance CPU/memory

**Auto-Scaling Rules:**
- Scale web instances when CPU > 70% for 5 minutes
- Scale workers when queue depth > 5,000 messages
- Scale down when CPU < 30% for 15 minutes

### Disaster Recovery

**Backup Strategy:**
- Database: Daily full backup + hourly incremental
- Redis: RDB snapshots every 6 hours
- Blob storage: Geo-redundant replication
- Configuration: Version controlled in Git

**Recovery Time Objective (RTO):** 1 hour
**Recovery Point Objective (RPO):** 1 hour

**Failover Procedures:**
1. Database failover to replica (automatic)
2. Redis failover to standby node (automatic)
3. RabbitMQ failover to cluster node (automatic)
4. Web/worker instances: Deploy to new region


## Migration Strategy

### Phase 1: Backward Compatible Changes (Week 1-2)

**Goal:** Add new fields and entities without breaking existing functionality

**Tasks:**
1. Add new columns to Messages table (MessageType, ConversationId, etc.)
2. Create new tables (Conversations, MediaAttachments, DeviceTokens)
3. Set default MessageType = OneWay for existing messages
4. Deploy database migration
5. Verify existing one-way messaging still works

**Rollback Plan:** Drop new columns and tables if issues arise

### Phase 2: Dual-Mode Support (Week 3-4)

**Goal:** Enable both one-way and chat messaging

**Tasks:**
1. Deploy new domain entities and services
2. Deploy new application services
3. Update SignalR hub for bidirectional messaging
4. Deploy new API endpoints
5. Keep existing endpoints working

**Testing:**
- Verify existing one-way messages work
- Test new chat functionality
- Load test with mixed traffic

### Phase 3: Infrastructure Enhancements (Week 5-6)

**Goal:** Add Redis, improve reliability, add media support

**Tasks:**
1. Deploy Redis cluster
2. Configure SignalR backplane
3. Migrate connection tracking to Redis
4. Add blob storage integration
5. Implement retry logic and DLQ
6. Add caching layer

**Monitoring:**
- Track cache hit rates
- Monitor queue depths
- Verify message delivery rates

### Phase 4: Push Notifications & Optimization (Week 7-8)

**Goal:** Add push notifications and performance optimizations

**Tasks:**
1. Integrate FCM and APNS
2. Implement device token management
3. Add database indexes
4. Optimize queries
5. Performance testing and tuning

**Success Criteria:**
- Push notifications delivered within 5 seconds
- API response time < 200ms (p95)
- Message throughput > 1,000/second

### Data Migration

**Existing Messages:**
```sql
-- Set MessageType for existing messages
UPDATE Messages 
SET MessageType = 0  -- OneWay
WHERE MessageType IS NULL;

-- Ensure ConversationId is null for one-way messages
UPDATE Messages 
SET ConversationId = NULL 
WHERE MessageType = 0;
```

**No data loss:** All existing messages remain accessible and functional


## Implementation Considerations

### Technology Choices

**Why Redis?**
- Fast in-memory cache for low latency
- Built-in support for SignalR backplane
- Pub/sub for distributed events
- Mature and battle-tested

**Why RabbitMQ?**
- Already integrated in the project
- Reliable message delivery guarantees
- Dead letter queue support
- Good performance for our scale

**Why Azure Blob Storage / AWS S3?**
- Cost-effective for large files
- Built-in CDN integration
- Geo-redundant storage
- Scalable to petabytes

**Why FsCheck for Property Testing?**
- Mature property-based testing library for .NET
- Integrates well with xUnit
- Supports custom generators
- Good documentation

### Breaking Changes

**None.** The design maintains full backward compatibility:
- Existing one-way messaging continues to work
- New MessageType field defaults to OneWay
- Existing API endpoints unchanged
- Existing database schema extended, not modified

### Future Enhancements

**Phase 5+ (Future):**
- Group chat support (multiple participants)
- Message reactions (emoji reactions)
- Message threading (reply to specific message)
- Voice messages
- Video calls integration
- Message search with Elasticsearch
- Analytics dashboard
- Chatbot integration
- Multi-language support for messages
- Message templates

### Known Limitations

1. **SignalR Scalability:** Limited to ~100K concurrent connections per Redis instance
   - **Mitigation:** Use Redis cluster with multiple instances

2. **Blob Storage Costs:** Can grow quickly with media
   - **Mitigation:** Implement retention policies, compress images

3. **Database Write Load:** High write volume on Messages table
   - **Mitigation:** Use write-optimized indexes, consider sharding for extreme scale

4. **Push Notification Delays:** FCM/APNS may have delays
   - **Mitigation:** This is acceptable for async notifications

5. **Message Ordering:** Not guaranteed across multiple workers
   - **Mitigation:** Use conversation ID partitioning for ordering within conversations


## Summary

This design provides a comprehensive solution for enhancing the Esh3arTech messaging system with:

✅ **Dual-Mode Messaging:** Support for both one-way read-only messages and bidirectional chat conversations
✅ **Media Sharing:** Images, videos, audio, and documents with thumbnail generation
✅ **Reliable Delivery:** Retry mechanisms, dead letter queue, and guaranteed delivery
✅ **High Scalability:** Horizontal scaling with Redis backplane and distributed state
✅ **High Availability:** Multi-instance deployment with health checks and failover
✅ **Low Latency:** Redis caching, database optimization, and CDN for media
✅ **Push Notifications:** FCM and APNS integration for offline users
✅ **Fault Tolerance:** Circuit breakers, retry policies, and graceful degradation
✅ **Backward Compatibility:** Existing one-way messaging continues to work unchanged

### Key Design Decisions

1. **Single Message Entity:** Use MessageType discriminator instead of separate entities
2. **Async Processing:** RabbitMQ for decoupling and reliability
3. **Distributed State:** Redis for SignalR backplane and connection tracking
4. **Phased Migration:** Incremental rollout to minimize risk
5. **ABP Framework Patterns:** Maintain DDD architecture and ABP conventions

### Next Steps

1. Review and approve this design document
2. Create detailed implementation tasks
3. Set up infrastructure (Redis, blob storage, etc.)
4. Begin Phase 1 implementation
5. Conduct thorough testing at each phase

---

**Design Document Version:** 1.0  
**Last Updated:** December 20, 2024  
**Status:** Ready for Review
