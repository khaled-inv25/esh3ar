# Implementation Plan: Enhanced Messaging System

## Phase 1: Foundation & Data Model (Weeks 1-2)

- [ ] 1. Set up project structure and dependencies
- [ ] 1.1 Add NuGet packages for Redis, FsCheck, and media processing

  - Add `StackExchange.Redis` for Redis caching and backplane
  - Add `FsCheck.Xunit` for property-based testing
  - Add `SixLabors.ImageSharp` for image thumbnail generation
  - Add `Azure.Storage.Blobs` or `AWSSDK.S3` for blob storage
  - _Requirements: 2.3, 2.4, 7.1, 8.4_

- [ ] 1.2 Update common.props and project references

  - Ensure all projects reference updated dependencies
  - Update target framework if needed
  - _Requirements: N/A (Infrastructure)_

- [ ] 2. Enhance Domain.Shared layer with new enums and constants
- [ ] 2.1 Create MessageType enum

  - Add `MessageType` enum with OneWay = 0, Chat = 1
  - Add to `Esh3arTech.Domain.Shared/Messages/MessageType.cs`
  - _Requirements: 1.1, 1.5, 1.12_

- [ ] 2.2 Create ConversationStatus enum

  - Add `ConversationStatus` enum with Active, Archived, Closed
  - Add to `Esh3arTech.Domain.Shared/Conversations/ConversationStatus.cs`
  - _Requirements: 1.11_

- [ ] 2.3 Create MediaType enum

  - Add `MediaType` enum with Image, Video, Audio, Document
  - Add to `Esh3arTech.Domain.Shared/Media/MediaType.cs`
  - _Requirements: 2.1_

- [ ] 2.4 Create DevicePlatform enum

  - Add `DevicePlatform` enum with Android, iOS
  - Add to `Esh3arTech.Domain.Shared/Devices/DevicePlatform.cs`
  - _Requirements: 3.1_

- [ ] 2.5 Add constants for new features

  - Add `ConversationConsts` with max lengths
  - Add `MediaConsts` with file size limits (50MB video, 10MB image, 5MB doc)
  - Add `DeviceTokenConsts` with token length limits
  - _Requirements: 2.2_

- [ ] 3. Enhance Message entity in Domain layer
- [ ] 3.1 Add new properties to Message entity

  - Add `MessageType` property with default OneWay
  - Add `ConversationId` nullable property
  - Add `SenderPhoneNumber` nullable property
  - Add `RetryCount` property with default 0
  - Add `DeliveredAt` nullable property
  - Add `ReadAt` nullable property
  - Add `FailureReason` nullable property
  - Add `Priority` property with default 1
  - Update `Esh3arTech.Domain/Messages/Message.cs`
  - _Requirements: 1.1, 1.5, 1.12, 4.1, 9.1, 9.3, 9.4, 5.2_

- [ ] 3.2 Add navigation property for MediaAttachments

  - Add `ICollection<MediaAttachment>` navigation property
  - _Requirements: 2.5, 2.7_

- [ ] 3.3 Add domain methods to Message entity

  - Add `SetMessageType(MessageType type)` method
  - Add `SetConversation(Guid conversationId)` method
  - Add `IncrementRetryCount()` method
  - Add `MarkAsDelivered()` method
  - Add `MarkAsRead()` method
  - Add ` (string reason)` method
  - _Requirements: 4.1, 9.2, 9.3, 9.4, 9.5_

- [ ]\* 3.4 Write property test for Message status transitions

  - **Property 3: Message Status Progression**
  - **Validates: Requirements 9.1, 9.2, 9.3, 9.4, 9.5**

- [ ] 4. Create Conversation entity in Domain layer
- [ ] 4.1 Create Conversation aggregate root

  - Create `Esh3arTech.Domain/Conversations/Conversation.cs`
  - Add properties: BusinessUserId, MobileUserPhoneNumber, LastMessageId, LastMessageAt
  - Add UnreadCountBusiness, UnreadCountMobile properties
  - Add Status property with default Active
  - Add navigation properties for Messages, BusinessUser, MobileUser
  - _Requirements: 1.6, 1.11, 1.13, 1.14_

- [ ] 4.2 Add domain methods to Conversation entity

  - Add `AddMessage(Message message)` method
  - Add `MarkAsRead(bool isByBusinessUser)` method
  - Add `Archive()` method
  - Add `Close()` method
  - Add `GetUnreadCount(bool forBusinessUser)` method
  - _Requirements: 1.6, 1.14_

- [ ]\* 4.3 Write property test for Conversation participant uniqueness

  - **Property 2: Conversation Participant Uniqueness**
  - **Validates: Requirements 1.11**

- [ ]\* 4.4 Write property test for unread count accuracy

  - **Property 6: Conversation Unread Count Accuracy**
  - **Validates: Requirements 1.14**

- [ ] 5. Create MediaAttachment entity in Domain layer
- [ ] 5.1 Create MediaAttachment entity

  - Create `Esh3arTech.Domain/Media/MediaAttachment.cs`
  - Add properties: MessageId, FileName, ContentType, FileSizeBytes
  - Add BlobStorageUrl, ThumbnailSmallUrl, ThumbnailMediumUrl, ThumbnailLargeUrl
  - Add MediaType property
  - Add navigation property for Message
  - _Requirements: 2.3, 2.4, 2.5_

- [ ]\* 5.2 Write property test for media file size validation

  - **Property 5: Media File Size Validation**
  - **Validates: Requirements 2.2**

- [ ] 6. Create DeviceToken entity in Domain layer
- [ ] 6.1 Create DeviceToken entity

  - Create `Esh3arTech.Domain/Devices/DeviceToken.cs`
  - Add properties: MobileUserPhoneNumber, Token, Platform
  - Add IsActive, FailureCount, LastUsedAt properties
  - Add navigation property for MobileUser
  - _Requirements: 3.1_

- [ ] 6.2 Add domain methods to DeviceToken entity

  - Add `MarkAsUsed()` method
  - Add `IncrementFailureCount()` method
  - Add `Deactivate()` method (after 3 failures)
  - _Requirements: 3.5_

- [ ]\* 6.3 Write property test for device token uniqueness

  - **Property 7: Device Token Uniqueness**
  - **Validates: Requirements 3.1**

- [ ] 7. Create database migration for new schema
- [ ] 7.1 Add DbSets to Esh3arTechDbContext

  - Add `DbSet<Conversation> Conversations`
  - Add `DbSet<MediaAttachment> MediaAttachments`
  - Add `DbSet<DeviceToken> DeviceTokens`
  - Update `Esh3arTech.EntityFrameworkCore/EntityFrameworkCore/Esh3arTechDbContext.cs`
  - _Requirements: N/A (Infrastructure)_

- [ ] 7.2 Create EF Core configurations

  - Create `ConversationConfiguration.cs` with fluent API mappings
  - Create `MediaAttachmentConfiguration.cs` with fluent API mappings
  - Create `DeviceTokenConfiguration.cs` with fluent API mappings
  - Update `MessageConfiguration.cs` to include new properties
  - Add indexes as per design document
  - _Requirements: N/A (Infrastructure)_

- [ ] 7.3 Generate and apply EF Core migration

  - Run `dotnet ef migrations add EnhancedMessagingSystem`
  - Review generated migration for correctness
  - Apply migration to development database
  - _Requirements: N/A (Infrastructure)_

- [ ] 7.4 Update existing Message records with default values

  - Set MessageType = OneWay for all existing messages
  - Ensure ConversationId is null for existing messages
  - _Requirements: 1.1_

- [ ] 8. Checkpoint - Verify data model
  - Ensure all tests pass
  - Verify database schema is correct
  - Confirm backward compatibility with existing messages
  - Ask user if questions arise

## Phase 2: Domain Services & Business Logic (Weeks 3-4)

- [ ] 9. Enhance MessageManager domain service
- [ ] 9.1 Update CreateMessageAsync for one-way messages

  - Modify existing method to set MessageType = OneWay
  - Ensure ConversationId remains null
  - Set Priority parameter (default = 1)
  - Update `Esh3arTech.Domain/Messages/MessageManager.cs`
  - _Requirements: 1.1, 1.10, 5.2_

- [ ] 9.2 Add CreateChatMessageAsync method

  - Create new method for chat messages
  - Set MessageType = Chat
  - Require ConversationId parameter
  - Set SenderPhoneNumber parameter
  - Validate conversation exists
  - _Requirements: 1.5, 1.9_

- [ ] 9.3 Add message status update methods

  - Add `UpdateStatus(Message message, MessageStatus newStatus)` method
  - Add `IncrementRetry(Message message)` method
  - Add `MarkAsFailed(Message message, string reason)` method
  - Implement status transition validation
  - _Requirements: 4.4, 9.2, 9.5_

- [ ]\* 9.4 Write property test for message type consistency

  - **Property 1: Message Type Consistency**
  - **Validates: Requirements 1.1, 1.12**

- [ ]\* 9.5 Write property test for retry count monotonicity

  - **Property 4: Retry Count Monotonicity**
  - **Validates: Requirements 4.1, 4.2**

- [ ] 10. Create ConversationManager domain service
- [ ] 10.1 Create ConversationManager class

  - Create `Esh3arTech.Domain/Conversations/ConversationManager.cs`
  - Inject IRepository<Conversation>, IRepository<MobileUser>
  - _Requirements: 1.6, 1.11_

- [ ] 10.2 Implement GetOrCreateConversationAsync method

  - Check if conversation exists between business user and mobile user
  - Create new conversation if not exists
  - Return existing conversation if found
  - Ensure only one active conversation per pair
  - _Requirements: 1.6, 1.11_

- [ ] 10.3 Implement AddMessageToConversationAsync method

  - Add message to conversation
  - Update LastMessageId and LastMessageAt
  - Increment unread count for recipient
  - _Requirements: 1.6_

- [ ] 10.4 Implement MarkAsReadAsync method

  - Reset unread count for specified participant
  - Update message statuses to Read
  - _Requirements: 1.14, 9.4_

- [ ]\* 10.5 Write property test for conversation operations

  - Test GetOrCreateConversation idempotency
  - Test AddMessage updates LastMessageAt correctly
  - Test MarkAsRead resets unread count

- [ ] 11. Create repository interfaces
- [ ] 11.1 Create IConversationRepository interface

  - Create `Esh3arTech.Domain/Conversations/IConversationRepository.cs`
  - Add `GetByParticipantsAsync(Guid businessUserId, string mobilePhone)` method
  - Add `GetActiveConversationsForBusinessUserAsync(Guid userId)` method
  - Add `GetActiveConversationsForMobileUserAsync(string phone)` method
  - _Requirements: 1.13, 1.14_

- [ ] 11.2 Implement ConversationRepository

  - Create `Esh3arTech.EntityFrameworkCore/EntityFrameworkCore/Conversations/ConversationRepository.cs`
  - Implement custom query methods with proper includes
  - Optimize queries with indexes
  - _Requirements: 1.13, 1.14_

- [ ] 12. Update permissions
- [ ] 12.1 Add new permissions to Esh3arTechPermissions

  - Add `Esh3arChat` permission for chat messaging
  - Add `Esh3arManageConversations` permission
  - Add `Esh3arUploadMedia` permission
  - Update `Esh3arTech.Application.Contracts/Permissions/Esh3arTechPermissions.cs`
  - _Requirements: 1.9, 1.10_

- [ ] 12.2 Update permission definition provider

  - Add permission definitions with display names and descriptions
  - Update `Esh3arTech.Application.Contracts/Permissions/Esh3arTechPermissionDefinitionProvider.cs`
  - _Requirements: 1.9, 1.10_

- [ ] 13. Checkpoint - Verify domain logic
  - Ensure all tests pass
  - Verify message creation for both types
  - Verify conversation management
  - Ask user if questions arise

## Phase 3: Application Services & DTOs (Weeks 5-6)

- [ ] 14. Create DTOs for new features
- [ ] 14.1 Create Conversation DTOs

  - Create `ConversationDto` with all properties
  - Create `ConversationInListDto` for list views
  - Create `CreateConversationDto` for input
  - Create `GetConversationsInput` for filtering
  - Add to `Esh3arTech.Application.Contracts/Conversations/`
  - _Requirements: 1.7, 1.13_

- [ ] 14.2 Create Media DTOs

  - Create `MediaAttachmentDto` with all properties
  - Create `UploadMediaDto` for file upload input
  - Create `ThumbnailSize` enum (Small, Medium, Large)
  - Add to `Esh3arTech.Application.Contracts/Media/`
  - _Requirements: 2.5, 2.6_

- [ ] 14.3 Create DeviceToken DTOs

  - Create `DeviceTokenDto` with all properties
  - Create `RegisterDeviceDto` for registration input
  - Create `UpdateDeviceTokenDto` for updates
  - Add to `Esh3arTech.Application.Contracts/Devices/`
  - _Requirements: 3.1_

- [ ] 14.4 Update Message DTOs

  - Add MessageType property to existing DTOs
  - Add ConversationId property
  - Add SenderPhoneNumber property
  - Add Priority property
  - Add MediaAttachments collection
  - Update `MessageDto`, `MessageInListDto`, `PendingMessageDto`
  - _Requirements: 1.12, 2.7_

- [ ] 14.5 Create new Message input DTOs

  - Create `SendChatMessageDto` for chat messages
  - Update `SendOneWayMessageDto` to include priority
  - Add media attachment support to both
  - _Requirements: 1.1, 1.5, 2.7_

- [ ] 15. Create IConversationAppService interface
- [ ] 15.1 Define conversation management methods

  - Create `Esh3arTech.Application.Contracts/Conversations/IConversationAppService.cs`
  - Add `GetOrCreateConversationAsync(CreateConversationDto input)` method
  - Add `GetConversationAsync(Guid id)` method
  - Add `GetConversationsAsync(GetConversationsInput input)` method
  - Add `MarkConversationAsReadAsync(Guid conversationId)` method
  - Add `ArchiveConversationAsync(Guid conversationId)` method
  - Add `CloseConversationAsync(Guid conversationId)` method
  - Add `GetUnreadConversationCountAsync()` method
  - _Requirements: 1.7, 1.13, 1.14_

- [ ] 16. Implement ConversationAppService
- [ ] 16.1 Create ConversationAppService class

  - Create `Esh3arTech.Application/Conversations/ConversationAppService.cs`
  - Inject ConversationManager, repositories, ObjectMapper
  - Inherit from Esh3arTechAppService
  - _Requirements: 1.6, 1.7_

- [ ] 16.2 Implement conversation retrieval methods

  - Implement `GetOrCreateConversationAsync` with authorization
  - Implement `GetConversationAsync` with authorization check
  - Implement `GetConversationsAsync` with pagination
  - Apply filters for business user or mobile user
  - _Requirements: 1.7, 1.13_

- [ ] 16.3 Implement conversation action methods

  - Implement `MarkConversationAsReadAsync`
  - Implement `ArchiveConversationAsync`
  - Implement `CloseConversationAsync`
  - Implement `GetUnreadConversationCountAsync`
  - _Requirements: 1.14_

- [ ]\* 16.4 Write unit tests for ConversationAppService

  - Test GetOrCreateConversation creates new conversation
  - Test GetConversations returns only user's conversations
  - Test MarkAsRead updates unread count
  - Test authorization checks

- [ ] 17. Enhance MessageAppService
- [ ] 17.1 Update SendOneWayMessageAsync method

  - Update to use enhanced MessageManager
  - Set MessageType = OneWay explicitly
  - Support priority parameter
  - Support media attachments
  - Update `Esh3arTech.Application/Messages/MessageAppService.cs`
  - _Requirements: 1.1, 1.2, 1.10, 2.7_

- [ ] 17.2 Add SendChatMessageAsync method

  - Create new method for chat messages
  - Get or create conversation
  - Create chat message via MessageManager
  - Publish event for delivery
  - _Requirements: 1.5, 1.9_

- [ ] 17.3 Add GetConversationMessagesAsync method

  - Retrieve messages for a specific conversation
  - Apply pagination
  - Order by creation time
  - Include media attachments
  - _Requirements: 1.7_

- [ ] 17.4 Update GetOneWayMessagesAsync method

  - Filter by MessageType = OneWay
  - Apply pagination
  - _Requirements: 1.8_

- [ ] 17.5 Update status tracking methods

  - Update `UpdateMessageStatusAsync` to handle all statuses
  - Add `MarkMessageAsReadAsync` method
  - Update timestamps (DeliveredAt, ReadAt)
  - Notify sender via SignalR if online
  - _Requirements: 9.2, 9.3, 9.4, 9.6_

- [ ]\* 17.6 Write unit tests for MessageAppService

  - Test SendOneWayMessage creates OneWay message
  - Test SendChatMessage creates Chat message with conversation
  - Test GetConversationMessages filters correctly
  - Test status updates work correctly

- [ ]\* 17.7 Write property test for one-way message read-only

  - **Property 12: One-Way Message Read-Only**
  - **Validates: Requirements 1.4**

- [ ]\* 17.8 Write property test for chat bidirectionality

  - **Property 13: Chat Message Bidirectionality**
  - **Validates: Requirements 1.5, 1.6**

- [ ] 18. Create IMediaAppService interface and implementation
- [ ] 18.1 Create IMediaAppService interface

  - Create `Esh3arTech.Application.Contracts/Media/IMediaAppService.cs`
  - Add `UploadMediaAsync(UploadMediaDto input)` method
  - Add `GetMediaUrlAsync(Guid mediaAttachmentId)` method
  - Add `GetThumbnailUrlAsync(Guid mediaAttachmentId, ThumbnailSize size)` method
  - Add `DeleteMediaAsync(Guid mediaAttachmentId)` method
  - _Requirements: 2.3, 2.6_

- [ ] 18.2 Implement MediaAppService

  - Create `Esh3arTech.Application/Media/MediaAppService.cs`
  - Inject IBlobContainer, IRepository<MediaAttachment>
  - _Requirements: 2.3_

- [ ] 18.3 Implement UploadMediaAsync method

  - Validate file type against allowed types
  - Validate file size against limits (50MB video, 10MB image, 5MB doc)
  - Upload to blob storage
  - Generate thumbnails for images (150x150, 300x300, 600x600)
  - Create MediaAttachment record
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5_

- [ ] 18.4 Implement GetMediaUrlAsync method

  - Generate time-limited signed URL (1 hour expiry)
  - Return URL for download
  - _Requirements: 2.6_

- [ ] 18.5 Implement GetThumbnailUrlAsync method

  - Return appropriate thumbnail URL based on size
  - Generate signed URL with 1 hour expiry
  - _Requirements: 2.4, 2.6_

- [ ]\* 18.6 Write unit tests for MediaAppService

  - Test file type validation
  - Test file size validation
  - Test thumbnail generation for images
  - Test signed URL generation

- [ ]\* 18.7 Write property test for signed URL expiration

  - **Property 11: Signed URL Expiration**
  - **Validates: Requirements 2.6**

- [ ] 19. Create IDeviceTokenAppService interface and implementation
- [ ] 19.1 Create IDeviceTokenAppService interface

  - Create `Esh3arTech.Application.Contracts/Devices/IDeviceTokenAppService.cs`
  - Add `RegisterDeviceAsync(RegisterDeviceDto input)` method
  - Add `UnregisterDeviceAsync(string token)` method
  - Add `UpdateDeviceTokenAsync(UpdateDeviceTokenDto input)` method
  - Add `GetUserDevicesAsync()` method
  - _Requirements: 3.1_

- [ ] 19.2 Implement DeviceTokenAppService

  - Create `Esh3arTech.Application/Devices/DeviceTokenAppService.cs`
  - Inject IRepository<DeviceToken>
  - Implement all interface methods
  - Validate device token uniqueness
  - _Requirements: 3.1_

- [ ]\* 19.3 Write unit tests for DeviceTokenAppService

  - Test device registration
  - Test token uniqueness validation
  - Test device unregistration

- [ ] 20. Update AutoMapper profiles
- [ ] 20.1 Add mappings for new entities

  - Add Conversation → ConversationDto mappings
  - Add MediaAttachment → MediaAttachmentDto mappings
  - Add DeviceToken → DeviceTokenDto mappings
  - Update Message mappings to include new properties
  - Update `Esh3arTech.Application/Esh3arTechApplicationAutoMapperProfile.cs`
  - _Requirements: N/A (Infrastructure)_

- [ ] 21. Checkpoint - Verify application services
  - Ensure all tests pass
  - Verify DTOs map correctly
  - Test API endpoints manually
  - Ask user if questions arise

## Phase 4: Real-time Communication & SignalR (Weeks 7-8)

- [ ] 22. Enhance SignalR hub for bidirectional messaging
- [ ] 22.1 Update OnlineMobileUserHub for chat support

  - Update `Esh3arTech.Web/Hubs/OnlineMobileUserHub.cs`
  - Add `SendChatMessage(SendChatMessageDto input)` hub method
  - Update `OnConnectedAsync` to handle both message types
  - Keep existing one-way message delivery
  - _Requirements: 1.2, 1.3, 1.5_

- [ ] 22.2 Add conversation-specific hub methods

  - Add `JoinConversation(Guid conversationId)` method
  - Add `LeaveConversation(Guid conversationId)` method
  - Add `TypingIndicator(Guid conversationId, bool isTyping)` method
  - _Requirements: 1.5_

- [ ] 22.3 Update message acknowledgment

  - Update `AcknowledgeMessage` to handle both message types
  - Update status to Delivered for one-way messages
  - Update status to Delivered/Read for chat messages
  - _Requirements: 1.15, 9.3_

- [ ]\* 22.4 Write integration tests for SignalR hub

  - Test one-way message delivery
  - Test chat message delivery
  - Test typing indicators
  - Test acknowledgments

- [ ] 23. Update event handlers for dual-mode messaging
- [ ] 23.1 Update DistributedSendMessageHandler

  - Update `Esh3arTech.Web/EventBus/DistributedHandlers/DistributedSendMessageHandler.cs`
  - Handle both OneWay and Chat message types
  - Route to appropriate SignalR method based on type
  - _Requirements: 1.2, 1.5_

- [ ] 23.2 Update LocalSendMessageHandler

  - Update `Esh3arTech.Application/Messages/Handlers/LocalSendMessageHandler.cs`
  - Publish to distributed event bus for cross-instance delivery
  - _Requirements: 1.2, 1.5_

- [ ]\* 23.3 Write integration tests for event handlers

  - Test local event publishing
  - Test distributed event handling
  - Test message routing

- [ ] 24. Implement distributed connection tracking with Redis
- [ ] 24.1 Create IDistributedConnectionTracker interface

  - Create `Esh3arTech.Web/SignalR/IDistributedConnectionTracker.cs`
  - Add `AddConnectionAsync(string phoneNumber, string connectionId)` method
  - Add `RemoveConnectionAsync(string phoneNumber, string connectionId)` method
  - Add `GetConnectionsAsync(string phoneNumber)` method
  - Add `IsOnlineAsync(string phoneNumber)` method
  - _Requirements: 7.2, 7.3_

- [ ] 24.2 Implement RedisConnectionTracker

  - Create `Esh3arTech.Web/SignalR/RedisConnectionTracker.cs`
  - Use Redis Sets to store connections per user
  - Set TTL of 5 minutes with auto-refresh on activity
  - Implement all interface methods
  - _Requirements: 7.2, 7.3_

- [ ] 24.3 Update OnlineMobileUserHub to use distributed tracker

  - Replace in-memory OnlineUserTrackerService with IDistributedConnectionTracker
  - Update OnConnectedAsync and OnDisconnectedAsync
  - _Requirements: 7.2_

- [ ]\* 24.4 Write integration tests for connection tracking

  - Test connection registration in Redis
  - Test connection removal
  - Test TTL expiration
  - Test cross-instance connection lookup

- [ ] 25. Configure Redis backplane for SignalR
- [ ] 25.1 Add Redis configuration to appsettings.json

  - Add Redis connection string
  - Add Redis instance name
  - Update `Esh3arTech.Web/appsettings.json`
  - _Requirements: 7.1_

- [ ] 25.2 Configure SignalR with Redis backplane

  - Update `Esh3arTech.Web/Esh3arTechWebModule.cs`
  - Add `services.AddSignalR().AddStackExchangeRedis()`
  - Configure Redis options
  - _Requirements: 7.1_

- [ ] 25.3 Configure distributed cache

  - Add `services.AddStackExchangeRedisCache()`
  - Configure cache options
  - _Requirements: 8.1_

- [ ]\* 25.4 Test multi-instance SignalR communication

  - Deploy two instances
  - Connect users to different instances
  - Verify messages route correctly via Redis backplane

- [ ] 26. Checkpoint - Verify real-time communication
  - Ensure all tests pass
  - Test SignalR with Redis backplane
  - Verify connection tracking works across instances
  - Ask user if questions arise

## Phase 5: Message Queuing & Reliability (Weeks 9-10)

- [ ] 27. Implement asynchronous message processing with RabbitMQ
- [ ] 27.1 Create message queue events

  - Create `MessageQueuedEvent` for enqueuing messages
  - Create `MessageDeliveryAttemptEvent` for retry attempts
  - Add to `Esh3arTech.Application.Contracts/Messages/Events/`
  - _Requirements: 5.1_

- [ ] 27.2 Update MessageAppService to enqueue messages

  - Modify SendOneWayMessageAsync to publish MessageQueuedEvent
  - Modify SendChatMessageAsync to publish MessageQueuedEvent
  - Return immediately after enqueuing (async processing)
  - Update status to Sent after enqueuing
  - _Requirements: 5.1, 8.3, 9.2_

- [ ] 27.3 Create MessageDeliveryWorker background service

  - Create `Esh3arTech.Application/Messages/Workers/MessageDeliveryWorker.cs`
  - Subscribe to MessageQueuedEvent
  - Process messages in batches of 100
  - Implement priority queue processing (High → Normal → Low)
  - _Requirements: 5.2, 5.3_

- [ ] 27.4 Implement message delivery logic in worker

  - Check if recipient is online via IDistributedConnectionTracker
  - If online: Send via SignalR
  - If offline: Send push notification and mark as Pending
  - Update message status accordingly
  - _Requirements: 1.2, 1.3, 3.2_

- [ ]\* 27.5 Write property test for priority queue ordering

  - **Property 9: Priority Queue Ordering**
  - **Validates: Requirements 5.2**

- [ ]\* 27.6 Write integration tests for message worker

  - Test batch processing
  - Test priority ordering
  - Test online/offline routing

- [ ] 28. Implement retry mechanism with exponential backoff
- [ ] 28.1 Create RetryPolicy service

  - Create `Esh3arTech.Application/Messages/Services/RetryPolicyService.cs`
  - Implement exponential backoff calculation (1s, 2s, 4s, 8s, 16s, 32s)
  - Maximum 6 retry attempts
  - _Requirements: 4.1_

- [ ] 28.2 Update MessageDeliveryWorker with retry logic

  - On delivery failure, increment retry count
  - Calculate next retry delay using RetryPolicy
  - Requeue message with delay
  - If retry count exceeds 6, move to dead letter queue
  - _Requirements: 4.1, 4.2_

- [ ] 28.3 Implement idempotency check

  - Use message ID as idempotency key
  - Check if message already processed before delivery
  - Skip processing if already delivered
  - _Requirements: 4.3, 10.3_

- [ ]\* 28.4 Write property test for message delivery idempotency

  - **Property 8: Message Delivery Idempotency**
  - **Validates: Requirements 4.3, 10.3**

- [ ]\* 28.5 Write unit tests for retry logic

  - Test exponential backoff calculation
  - Test retry count increment
  - Test max retry limit

- [ ] 29. Implement dead letter queue
- [ ] 29.1 Configure RabbitMQ dead letter queue

  - Update `Esh3arTech.Web/Esh3arTechWebModule.cs`
  - Configure dead letter exchange and queue
  - Set message TTL and retention policy (7 days)
  - _Requirements: 4.2, 4.5_

- [ ] 29.2 Create DeadLetterQueueService

  - Create `Esh3arTech.Application/Messages/Services/DeadLetterQueueService.cs`
  - Implement `MoveToDeadLetterQueueAsync(Message message, string reason)` method
  - Log failure details
  - Send admin notification email
  - _Requirements: 4.2, 4.5_

- [ ] 29.3 Create admin UI for DLQ management

  - Create Razor page for viewing dead letter messages
  - Add ability to requeue messages
  - Add bulk operations
  - Add to `Esh3arTech.Web/Pages/Messages/DeadLetterQueue.cshtml`
  - _Requirements: 4.5_

- [ ]\* 29.4 Write integration tests for DLQ

  - Test message moves to DLQ after max retries
  - Test admin notification
  - Test requeue functionality

- [ ] 30. Implement rate limiting
- [ ] 30.1 Configure rate limiting middleware

  - Add rate limiting to `Esh3arTech.Web/Esh3arTechWebModule.cs`
  - Configure per-user limits (100 messages/minute)
  - Configure global limits (10,000 messages/second)
  - _Requirements: 5.4, 5.5_

- [ ] 30.2 Add rate limiting to message endpoints

  - Apply rate limiter to SendOneWayMessageAsync
  - Apply rate limiter to SendChatMessageAsync
  - Return 429 Too Many Requests when limit exceeded
  - _Requirements: 5.5_

- [ ] 30.3 Implement queue depth monitoring

  - Monitor RabbitMQ queue depth
  - Apply backpressure when depth exceeds 10,000
  - Log warnings and alert admins
  - _Requirements: 5.4_

- [ ]\* 30.4 Write integration tests for rate limiting

  - Test per-user rate limit enforcement
  - Test global rate limit
  - Test backpressure mechanism

- [ ] 31. Checkpoint - Verify message queuing and reliability
  - Ensure all tests pass
  - Test retry mechanism with failures
  - Verify DLQ functionality
  - Test rate limiting
  - Ask user if questions arise

## Phase 6: Push Notifications (Weeks 11-12)

- [ ] 32. Set up push notification infrastructure
- [ ] 32.1 Add Firebase Admin SDK NuGet package

  - Add `FirebaseAdmin` package
  - Add configuration for FCM credentials
  - _Requirements: 3.2_

- [ ] 32.2 Add APNS client NuGet package

  - Add appropriate APNS client library
  - Add configuration for APNS certificates
  - _Requirements: 3.2_

- [ ] 32.3 Configure push notification services

  - Add FCM and APNS configuration to appsettings.json
  - Register services in `Esh3arTechWebModule.cs`
  - _Requirements: 3.1, 3.2_

- [ ] 33. Implement push notification services
- [ ] 33.1 Create IPushNotificationService interface

  - Create `Esh3arTech.Application.Contracts/Notifications/IPushNotificationService.cs`
  - Add `SendNotificationAsync(string token, PushNotificationDto notification)` method
  - Add `SendToMultipleDevicesAsync(List<string> tokens, PushNotificationDto notification)` method
  - _Requirements: 3.2, 3.4_

- [ ] 33.2 Implement FcmPushNotificationService

  - Create `Esh3arTech.Application/Notifications/FcmPushNotificationService.cs`
  - Implement FCM notification sending
  - Handle FCM errors and token invalidation
  - _Requirements: 3.2, 3.5_

- [ ] 33.3 Implement ApnsPushNotificationService

  - Create `Esh3arTech.Application/Notifications/ApnsPushNotificationService.cs`
  - Implement APNS notification sending
  - Handle APNS errors and token invalidation
  - _Requirements: 3.2, 3.5_

- [ ] 33.4 Create PushNotificationFactory

  - Create factory to select appropriate service based on platform
  - Inject both FCM and APNS services
  - _Requirements: 3.2_

- [ ]\* 33.5 Write unit tests for push notification services

  - Test FCM notification formatting
  - Test APNS notification formatting
  - Test error handling
  - Test token invalidation after 3 failures

- [ ] 34. Integrate push notifications with message delivery
- [ ] 34.1 Update MessageDeliveryWorker to send push notifications

  - When recipient is offline, get device tokens
  - Send push notification to all active devices
  - Include message preview (sender name, first 100 chars)
  - Include conversation ID for deep linking
  - _Requirements: 3.2, 3.3, 3.4_

- [ ] 34.2 Implement notification preference checking

  - Check if user has disabled notifications
  - Skip push notification if disabled
  - _Requirements: 3.6_

- [ ] 34.3 Track push notification delivery status

  - Log successful deliveries
  - Log failures and increment device token failure count
  - Deactivate token after 3 consecutive failures
  - _Requirements: 3.5, 3.7_

- [ ]\* 34.4 Write property test for push notification delivery

  - **Property 15: Push Notification Delivery**
  - **Validates: Requirements 3.2, 3.4**

- [ ]\* 34.5 Write integration tests for push notification flow

  - Test notification sent when user offline
  - Test notification to multiple devices
  - Test notification preference respected
  - Test token deactivation on failures

- [ ] 35. Checkpoint - Verify push notifications
  - Ensure all tests pass
  - Test FCM notifications on Android device
  - Test APNS notifications on iOS device
  - Verify token management
  - Ask user if questions arise

## Phase 7: Performance & Caching (Weeks 13-14)

- [ ] 36. Implement Redis caching layer
- [ ] 36.1 Create caching service interfaces

  - Create `IConversationCacheService` interface
  - Create `IMessageCacheService` interface
  - Add to `Esh3arTech.Application.Contracts/Caching/`
  - _Requirements: 8.1_

- [ ] 36.2 Implement ConversationCacheService

  - Create `Esh3arTech.Application/Caching/ConversationCacheService.cs`
  - Cache conversation list per user (5 min TTL)
  - Cache unread counts (5 min TTL)
  - Implement cache invalidation on updates
  - _Requirements: 8.1, 8.2_

- [ ] 36.3 Implement MessageCacheService

  - Create `Esh3arTech.Application/Caching/MessageCacheService.cs`
  - Cache last 50 messages per conversation (5 min TTL)
  - Implement cache invalidation on new messages
  - _Requirements: 8.1_

- [ ] 36.4 Update application services to use caching

  - Update ConversationAppService to check cache first
  - Update MessageAppService to check cache first
  - Implement cache-aside pattern
  - Fall back to database on cache miss
  - _Requirements: 8.1, 8.7_

- [ ]\* 36.5 Write property test for cache-database consistency

  - **Property 10: Cache-Database Consistency**
  - **Validates: Requirements 10.4**

- [ ]\* 36.6 Write integration tests for caching

  - Test cache hit scenario
  - Test cache miss scenario
  - Test cache invalidation
  - Test fallback to database

- [ ] 37. Optimize database queries
- [ ] 37.1 Add database indexes

  - Add composite index on (ConversationId, CreationTime) for Messages
  - Add composite index on (RecipientPhoneNumber, Status) for Messages
  - Add composite index on (BusinessUserId, LastMessageAt) for Conversations
  - Add composite index on (MobileUserPhoneNumber, LastMessageAt) for Conversations
  - Create migration for indexes
  - _Requirements: 8.5_

- [ ] 37.2 Optimize query projections

  - Use `Select()` to project only needed columns
  - Use `AsNoTracking()` for read-only queries
  - Avoid N+1 queries with proper `Include()`
  - _Requirements: 8.2_

- [ ] 37.3 Implement cursor-based pagination

  - Replace offset pagination with cursor-based
  - Use CreationTime or Id as cursor
  - Improves performance for large datasets
  - Update GetConversationsAsync and GetMessagesAsync
  - _Requirements: 8.6_

- [ ]\* 37.4 Write performance tests

  - Test query performance with 10K messages
  - Test pagination performance
  - Verify p95 latency < 200ms

- [ ] 38. Configure blob storage CDN
- [ ] 38.1 Set up Azure CDN or CloudFront

  - Configure CDN for blob storage account
  - Set up edge caching rules
  - Configure cache TTL (1 hour for media)
  - _Requirements: 8.4_

- [ ] 38.2 Update MediaAppService to use CDN URLs

  - Generate CDN URLs instead of direct blob URLs
  - Update GetMediaUrlAsync method
  - Update GetThumbnailUrlAsync method
  - _Requirements: 8.4_

- [ ]\* 38.3 Test CDN performance

  - Verify media loads from edge locations
  - Test cache hit rates
  - Measure latency improvement

- [ ] 39. Implement message partitioning for parallel processing
- [ ] 39.1 Configure RabbitMQ partitioning

  - Partition messages by ConversationId for chat messages
  - Partition messages by RecipientPhoneNumber for one-way messages
  - Configure multiple queues for parallel processing
  - _Requirements: 5.6_

- [ ] 39.2 Update MessageDeliveryWorker for partitioned queues

  - Subscribe to multiple partitioned queues
  - Process partitions in parallel
  - Maintain ordering within partitions
  - _Requirements: 5.6, 5.7_

- [ ]\* 39.3 Write performance tests for partitioning

  - Test throughput with partitioning
  - Verify message ordering within conversations
  - Test parallel processing

- [ ] 40. Checkpoint - Verify performance optimizations
  - Ensure all tests pass
  - Run load tests (1000+ messages/second)
  - Verify p95 latency < 200ms
  - Verify cache hit rates > 80%
  - Ask user if questions arise

## Phase 8: Fault Tolerance & High Availability (Weeks 15-16)

- [ ] 41. Implement circuit breaker pattern with Polly
- [ ] 41.1 Add Polly NuGet packages

  - Add `Polly` and `Polly.Extensions.Http` packages
  - _Requirements: 6.2_

- [ ] 41.2 Configure circuit breakers for external services

  - Add circuit breaker for push notification services (5 failures, 1 min break)
  - Add circuit breaker for blob storage (3 failures, 30 sec break)
  - Add circuit breaker for Redis (3 failures, 30 sec break)
  - Configure in `Esh3arTechWebModule.cs`
  - _Requirements: 6.2_

- [ ] 41.3 Implement fallback strategies

  - Fallback to database when Redis unavailable
  - Queue operations for retry when external services down
  - Log circuit breaker state changes
  - _Requirements: 6.3, 8.7_

- [ ]\* 41.4 Write integration tests for circuit breakers

  - Test circuit opens after failures
  - Test circuit closes after recovery
  - Test fallback strategies

- [ ] 42. Implement health checks
- [ ] 42.1 Create custom health checks

  - Create `DatabaseHealthCheck` for SQL Server
  - Create `RedisHealthCheck` for Redis
  - Create `RabbitMqHealthCheck` for RabbitMQ
  - Create `BlobStorageHealthCheck` for blob storage
  - Create `SignalRHealthCheck` for SignalR hub
  - Add to `Esh3arTech.Web/HealthChecks/`
  - _Requirements: 6.4_

- [ ] 42.2 Configure health check endpoints

  - Add health check endpoint at `/health`
  - Add detailed health check endpoint at `/health/details`
  - Configure health check UI
  - Update `Esh3arTechWebModule.cs`
  - _Requirements: 6.4, 6.5_

- [ ] 42.3 Configure health check failure responses

  - Return 503 Service Unavailable when unhealthy
  - Stop accepting new requests when unhealthy
  - Allow in-flight requests to complete
  - _Requirements: 6.5_

- [ ]\* 42.4 Write integration tests for health checks

  - Test all health checks pass when healthy
  - Test health checks fail when dependencies down
  - Test 503 response when unhealthy

- [ ] 43. Implement graceful shutdown
- [ ] 43.1 Configure graceful shutdown in Program.cs

  - Set shutdown timeout (30 seconds)
  - Register shutdown handlers
  - Update `Esh3arTech.Web/Program.cs`
  - _Requirements: 6.6_

- [ ] 43.2 Implement shutdown logic in MessageDeliveryWorker

  - Stop accepting new messages
  - Complete processing of in-flight messages
  - Requeue unprocessed messages
  - _Requirements: 6.6_

- [ ] 43.3 Implement shutdown logic in SignalR hub

  - Notify connected clients of shutdown
  - Allow clients to reconnect to other instances
  - Close connections gracefully
  - _Requirements: 6.6_

- [ ]\* 43.4 Test graceful shutdown

  - Test in-flight messages complete
  - Test no message loss during shutdown
  - Test clients reconnect successfully

- [ ] 44. Implement bulkhead isolation
- [ ] 44.1 Configure bulkhead policies with Polly

  - Isolate push notification operations (max 10 concurrent)
  - Isolate blob storage operations (max 20 concurrent)
  - Isolate database operations (max 50 concurrent)
  - _Requirements: 6.7_

- [ ] 44.2 Apply bulkhead policies to services

  - Wrap push notification calls with bulkhead
  - Wrap blob storage calls with bulkhead
  - Wrap expensive database queries with bulkhead
  - _Requirements: 6.7_

- [ ]\* 44.3 Write tests for bulkhead isolation

  - Test concurrent operation limits
  - Test queue behavior when limit reached
  - Test isolation between bulkheads

- [ ] 45. Configure load balancer
- [ ] 45.1 Create Nginx configuration

  - Configure upstream servers
  - Configure health check endpoint
  - Configure load balancing method (least_conn)
  - Configure sticky sessions for SignalR
  - Create `nginx.conf` file
  - _Requirements: 6.1, 7.5_

- [ ] 45.2 Configure SSL/TLS termination

  - Configure SSL certificates
  - Configure HTTPS redirect
  - Configure security headers
  - _Requirements: N/A (Security)_

- [ ] 45.3 Configure connection draining

  - Configure graceful shutdown on scale-down
  - Set drain timeout (60 seconds)
  - _Requirements: 7.6_

- [ ]\* 45.4 Test load balancer configuration

  - Test traffic distribution
  - Test health check integration
  - Test failover on instance failure
  - Test connection draining

- [ ] 46. Checkpoint - Verify fault tolerance and HA
  - Ensure all tests pass
  - Test circuit breakers with simulated failures
  - Test health checks and graceful shutdown
  - Test load balancer failover
  - Ask user if questions arise

## Phase 9: Monitoring, Logging & Security (Weeks 17-18)

- [ ] 47. Implement comprehensive logging
- [ ] 47.1 Configure structured logging with Serilog

  - Already configured, enhance with additional sinks
  - Add Application Insights sink (Azure) or CloudWatch (AWS)
  - Configure log levels per namespace
  - Update `Esh3arTech.Web/Program.cs`
  - _Requirements: N/A (Observability)_

- [ ] 47.2 Add logging to critical operations

  - Log message creation with context (messageId, type, recipient)
  - Log delivery attempts with retry count
  - Log failures with exception details
  - Log performance metrics (processing time)
  - _Requirements: 4.7_

- [ ] 47.3 Implement correlation IDs

  - Use ABP's built-in correlation ID
  - Include in all log entries
  - Pass through SignalR and event bus
  - _Requirements: N/A (Observability)_

- [ ]\* 47.4 Review log output

  - Verify structured logging format
  - Verify sensitive data not logged
  - Verify log levels appropriate

- [ ] 48. Implement metrics and monitoring
- [ ] 48.1 Configure Application Insights or CloudWatch

  - Add telemetry client
  - Configure custom metrics
  - Configure alerts
  - _Requirements: N/A (Observability)_

- [ ] 48.2 Track business metrics

  - Messages sent per minute/hour/day
  - Message delivery success rate
  - Average delivery time
  - Conversation creation rate
  - Active conversations count
  - Media upload volume
  - _Requirements: N/A (Observability)_

- [ ] 48.3 Track technical metrics

  - Queue depth and processing rate
  - Cache hit/miss ratio
  - Database query performance
  - SignalR connection count
  - API response times (p50, p95, p99)
  - Error rates by type
  - _Requirements: 5.7, 8.2_

- [ ] 48.4 Configure alerting rules

  - Critical: Database down, Redis down, RabbitMQ down
  - Critical: Message delivery success rate < 95%
  - Critical: API error rate > 5%
  - Warning: Queue depth > 10,000
  - Warning: Cache hit rate < 80%
  - Warning: Average delivery time > 5 seconds
  - _Requirements: N/A (Observability)_

- [ ]\* 48.5 Test monitoring and alerts

  - Trigger test alerts
  - Verify metrics collection
  - Verify dashboard displays correctly

- [ ] 49. Implement security enhancements
- [ ] 49.1 Add input validation

  - Validate message content length (max 10,000 chars)
  - Sanitize HTML/script tags
  - Validate phone number format
  - Validate file types and sizes
  - _Requirements: N/A (Security)_

- [ ] 49.2 Implement data encryption

  - Ensure TLS 1.2+ for all connections
  - Encrypt sensitive configuration (use Azure Key Vault or AWS Secrets Manager)
  - Consider encrypting message content at rest (optional)
  - _Requirements: N/A (Security)_

- [ ] 49.3 Implement audit logging

  - Log all PII access (phone numbers)
  - Log permission changes
  - Log admin actions (DLQ requeue, etc.)
  - _Requirements: N/A (Security)_

- [ ] 49.4 Implement GDPR compliance features

  - Add data export functionality
  - Add data deletion functionality (right to be forgotten)
  - Add data retention policies
  - _Requirements: N/A (Compliance)_

- [ ]\* 49.5 Security testing

  - Test input validation
  - Test authorization checks
  - Test rate limiting
  - Perform basic penetration testing

- [ ] 50. Create admin dashboard
- [ ] 50.1 Create monitoring dashboard page

  - Display real-time metrics
  - Display queue depths
  - Display active connections
  - Display error rates
  - Add to `Esh3arTech.Web/Pages/Admin/Dashboard.cshtml`
  - _Requirements: N/A (Admin Tools)_

- [ ] 50.2 Create message management page

  - View recent messages
  - Filter by type, status, user
  - View message details
  - Retry failed messages
  - Add to `Esh3arTech.Web/Pages/Admin/Messages.cshtml`
  - _Requirements: N/A (Admin Tools)_

- [ ] 50.3 Create conversation management page

  - View all conversations
  - Filter by status, user
  - View conversation details
  - Archive/close conversations
  - Add to `Esh3arTech.Web/Pages/Admin/Conversations.cshtml`
  - _Requirements: N/A (Admin Tools)_

- [ ] 51. Checkpoint - Verify monitoring and security
  - Ensure all tests pass
  - Verify logging works correctly
  - Verify metrics collection
  - Verify security measures
  - Ask user if questions arise

## Phase 10: Testing & Documentation (Weeks 19-20)

- [ ] 52. Comprehensive property-based testing
- [ ] 52.1 Write property test for offline message queuing

  - **Property 14: Offline Message Queuing**
  - **Validates: Requirements 1.3, 1.15**

- [ ] 52.2 Verify all 15 correctness properties are tested

  - Review all property tests implemented
  - Ensure each runs minimum 100 iterations
  - Ensure each is tagged with property number and requirement
  - _Requirements: All_

- [ ] 52.3 Run full property test suite

  - Execute all property tests
  - Fix any failures
  - Document any edge cases discovered
  - _Requirements: All_

- [ ] 53. Integration and end-to-end testing
- [ ] 53.1 Write end-to-end test for one-way messaging flow

  - Business user sends one-way message
  - Message queued and processed
  - Online mobile user receives via SignalR
  - Offline mobile user receives on reconnect
  - _Requirements: 1.1, 1.2, 1.3_

- [ ] 53.2 Write end-to-end test for chat messaging flow

  - Mobile user initiates chat
  - Conversation created
  - Business user receives message
  - Business user replies
  - Mobile user receives reply
  - _Requirements: 1.5, 1.6, 1.7_

- [ ] 53.3 Write end-to-end test for media sharing

  - User uploads image
  - Thumbnails generated
  - Media attached to message
  - Recipient receives message with media
  - Recipient downloads media via signed URL
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6_

- [ ] 53.4 Write end-to-end test for push notifications

  - User goes offline
  - Message sent to offline user
  - Push notification delivered to device
  - User comes online
  - Pending message delivered via SignalR
  - _Requirements: 3.2, 3.3, 3.4_

- [ ] 53.5 Write end-to-end test for retry and DLQ

  - Message delivery fails
  - System retries with exponential backoff
  - After max retries, message moves to DLQ
  - Admin requeues message from DLQ
  - Message successfully delivered
  - _Requirements: 4.1, 4.2, 4.5_

- [ ]\* 53.6 Run full integration test suite

  - Execute all integration tests
  - Fix any failures
  - Verify all scenarios work end-to-end

- [ ] 54. Performance and load testing
- [ ] 54.1 Set up load testing environment

  - Configure test environment with multiple instances
  - Set up load testing tools (k6, JMeter, or Artillery)
  - _Requirements: 5.7, 8.2_

- [ ] 54.2 Run load tests for message throughput

  - Test 1,000 messages/second
  - Test 5,000 messages/second
  - Test 10,000 messages/second
  - Verify system handles load without degradation
  - _Requirements: 5.7_

- [ ] 54.3 Run load tests for concurrent connections

  - Test 10,000 concurrent SignalR connections
  - Test 50,000 concurrent connections
  - Test 100,000 concurrent connections
  - Verify Redis backplane scales appropriately
  - _Requirements: 7.1, 7.5_

- [ ] 54.4 Run latency tests

  - Measure API response times under load
  - Verify p95 latency < 200ms
  - Verify p99 latency < 500ms
  - _Requirements: 8.2, 8.3, 5.7_

- [ ] 54.5 Run stress tests

  - Test system behavior under extreme load
  - Test graceful degradation
  - Test recovery after load reduction
  - _Requirements: 6.3_

- [ ]\* 54.6 Document performance test results

  - Record baseline metrics
  - Document bottlenecks found
  - Document optimizations applied
  - Record final metrics

- [ ] 55. Create technical documentation
- [ ] 55.1 Document architecture and design decisions

  - Create architecture diagram
  - Document technology choices and rationale
  - Document data model and relationships
  - Document message flows
  - Add to project wiki or docs folder
  - _Requirements: N/A (Documentation)_

- [ ] 55.2 Document API endpoints

  - Document all message endpoints
  - Document all conversation endpoints
  - Document all media endpoints
  - Document all device token endpoints
  - Include request/response examples
  - Update Swagger documentation
  - _Requirements: N/A (Documentation)_

- [ ] 55.3 Document deployment procedures

  - Document infrastructure setup (Redis, RabbitMQ, blob storage)
  - Document configuration requirements
  - Document deployment steps
  - Document scaling procedures
  - Document disaster recovery procedures
  - _Requirements: N/A (Documentation)_

- [ ] 55.4 Document monitoring and troubleshooting

  - Document key metrics to monitor
  - Document common issues and solutions
  - Document log locations and formats
  - Document alerting procedures
  - _Requirements: N/A (Documentation)_

- [ ] 55.5 Create developer onboarding guide

  - Document local development setup
  - Document how to run tests
  - Document code organization
  - Document contribution guidelines
  - _Requirements: N/A (Documentation)_

- [ ] 56. Create user documentation
- [ ] 56.1 Document one-way messaging for business users

  - How to send one-way messages
  - How to send broadcast messages
  - How to attach media
  - How to view message status
  - _Requirements: N/A (Documentation)_

- [ ] 56.2 Document chat messaging for business users

  - How to start a conversation
  - How to send chat messages
  - How to view conversation history
  - How to manage conversations
  - _Requirements: N/A (Documentation)_

- [ ] 56.3 Document mobile user features

  - How to receive one-way messages
  - How to start a chat conversation
  - How to send messages
  - How to view media
  - How to manage notifications
  - _Requirements: N/A (Documentation)_

- [ ] 57. Final checkpoint - Complete system verification
  - Ensure all tests pass (unit, integration, property, e2e)
  - Verify all requirements are met
  - Verify all correctness properties validated
  - Run full regression test suite
  - Perform final code review
  - Update all documentation
  - Ask user if questions arise

## Phase 11: Deployment & Go-Live (Week 21)

- [ ] 58. Prepare production environment
- [ ] 58.1 Set up production infrastructure

  - Provision Redis cluster (3 nodes)
  - Provision RabbitMQ cluster (3 nodes)
  - Provision SQL Server with read replicas
  - Provision blob storage with geo-redundancy
  - Configure CDN
  - _Requirements: N/A (Infrastructure)_

- [ ] 58.2 Configure production settings

  - Update connection strings
  - Configure SSL certificates
  - Configure push notification credentials
  - Configure monitoring and alerting
  - Set up backup procedures
  - _Requirements: N/A (Infrastructure)_

- [ ] 58.3 Deploy load balancer

  - Deploy Nginx or cloud load balancer
  - Configure health checks
  - Configure SSL termination
  - Test failover
  - _Requirements: 6.1, 7.5_

- [ ] 59. Deploy application
- [ ] 59.1 Deploy database migrations

  - Run migrations on production database
  - Verify schema changes
  - Verify data migration (existing messages)
  - _Requirements: N/A (Deployment)_

- [ ] 59.2 Deploy application instances

  - Deploy 3 web instances behind load balancer
  - Deploy 3 worker instances for message processing
  - Verify all instances healthy
  - Verify SignalR backplane working
  - _Requirements: 7.1, 7.5_

- [ ] 59.3 Smoke test production deployment

  - Test one-way message sending
  - Test chat message sending
  - Test media upload
  - Test push notifications
  - Test failover scenarios
  - _Requirements: All_

- [ ] 60. Monitor and optimize
- [ ] 60.1 Monitor system for first 24 hours

  - Watch for errors and exceptions
  - Monitor performance metrics
  - Monitor queue depths
  - Monitor resource utilization
  - _Requirements: N/A (Operations)_

- [ ] 60.2 Tune configuration based on real traffic

  - Adjust cache TTLs if needed
  - Adjust rate limits if needed
  - Adjust worker pool sizes if needed
  - Adjust database connection pools if needed
  - _Requirements: N/A (Operations)_

- [ ] 60.3 Create runbook for operations team

  - Document common operational tasks
  - Document incident response procedures
  - Document escalation procedures
  - Document on-call procedures
  - _Requirements: N/A (Operations)_

- [ ] 61. Final sign-off
  - Verify all requirements met
  - Verify all tests passing
  - Verify documentation complete
  - Obtain stakeholder approval
  - Celebrate successful delivery! 🎉

---

## Summary

**Total Duration:** 21 weeks (approximately 5 months)

**Phases:**

1. Foundation & Data Model (2 weeks)
2. Domain Services & Business Logic (2 weeks)
3. Application Services & DTOs (2 weeks)
4. Real-time Communication & SignalR (2 weeks)
5. Message Queuing & Reliability (2 weeks)
6. Push Notifications (2 weeks)
7. Performance & Caching (2 weeks)
8. Fault Tolerance & High Availability (2 weeks)
9. Monitoring, Logging & Security (2 weeks)
10. Testing & Documentation (2 weeks)
11. Deployment & Go-Live (1 week)

**Key Milestones:**

- Week 2: Data model complete
- Week 4: Domain logic complete
- Week 6: Application services complete
- Week 8: Real-time messaging complete
- Week 10: Reliability features complete
- Week 12: Push notifications complete
- Week 14: Performance optimizations complete
- Week 16: High availability complete
- Week 18: Security and monitoring complete
- Week 20: Testing complete
- Week 21: Production deployment

**Testing Strategy:**

- Unit tests throughout development
- Property-based tests for correctness properties (15 properties)
- Integration tests for component interactions
- End-to-end tests for user scenarios
- Performance and load tests before deployment

**Optional Tasks:** Marked with `*` - can be skipped for faster MVP delivery
