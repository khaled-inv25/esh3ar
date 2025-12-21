# Implementation Plan: Guaranteed Message Delivery

## Overview

This implementation plan breaks down the guaranteed message delivery system into incremental, testable tasks. The approach prioritizes core message delivery functionality first, then adds reliability features (retry logic, distributed locking), and finally implements scalability and monitoring features.

**Current Status**: Basic messaging infrastructure exists with Message entity, MessageAppService, SignalR hub, and RabbitMQ integration. This plan focuses on adding guaranteed delivery features, reliability mechanisms, and comprehensive testing.

## Tasks

- [ ] 1. Enhance Message Domain Model and Database Schema
  - Add missing fields to Message entity: SenderId (Guid), RecipientId (Guid), SentAt, LastRetryAt, IdempotencyKey, ContentType (MessageContentType enum)
  - Update existing RecipientPhoneNumber to be optional (keep for backward compatibility)
  - Add methods to Message entity: MarkAsSent(), MarkAsDelivered(), MarkAsRead(), MarkAsFailed(string reason), IncrementRetryCount(), CanRetry(int maxRetries)
  - Create MessageAttachment entity with properties: MessageId, FileName, ContentType, FileSize, BlobName, AccessUrl, UrlExpiresAt
  - Update MessageConfiguration to add database indexes: IX_Messages_RecipientId_Status, IX_Messages_Status_CreationTime, IX_Messages_IdempotencyKey
  - Create and apply EF Core migration
  - _Requirements: 1.2, 1.5, 4.1, 4.4, 5.5, 6.1, 11.1_

- [ ]* 1.1 Write property test for message persistence
  - **Property 2: Message Persistence is Immediate**
  - **Validates: Requirements 1.2**

- [ ]* 1.2 Write property test for unique message identifiers
  - **Property 1: Message Creation Returns Unique Identifiers**
  - **Validates: Requirements 1.1, 11.1**

- [ ] 2. Enhance Message Manager Domain Service
  - [ ] 2.1 Update CreateOneWayMessageAsync method
    - Update to accept SenderId (Guid) and RecipientId (Guid) instead of phone numbers
    - Add validation for recipient existence
    - Generate idempotency keys using GuidGenerator
    - Set initial message status to "Queued"
    - Add support for media attachment metadata (file validation only, upload in task 14)
    - _Requirements: 1.1, 1.3, 1.4, 1.5, 11.1_

  - [ ]* 2.2 Write property tests for message creation
    - **Property 4: Invalid Messages Are Not Queued**
    - **Property 5: Initial Message State is Correct**
    - **Validates: Requirements 1.3, 1.4, 1.5**

  - [ ] 2.3 Implement distributed locking methods
    - Add AcquireMessageLockAsync using IDistributedCache<MessageLock>
    - Add ReleaseMessageLockAsync
    - Set lock lease timeout to 30 seconds
    - Create MessageLock cache item class
    - _Requirements: 9.3, 11.2, 11.3_

  - [ ]* 2.4 Write property test for distributed locking
    - **Property 25: Distributed Locking Prevents Duplicates**
    - **Property 26: Lock Release on Worker Failure**
    - **Validates: Requirements 9.3, 11.2, 11.3**

  - [ ] 2.5 Implement pending message retrieval
    - Add GetPendingMessagesForUserAsync(Guid userId)
    - Query messages with Status = Pending and RecipientId = userId
    - Order by CreationTime ascending
    - _Requirements: 3.3, 3.4_

  - [ ] 2.6 Implement dead letter queue operations
    - Add MoveToDeadLetterQueueAsync(Guid messageId, string reason)
    - Update message status to "Failed"
    - Set FailureReason
    - Log failure details with structured logging
    - _Requirements: 5.3, 5.4, 13.1_

- [ ] 3. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 4. Enhance Online User Tracker Service
  - [ ] 4.1 Update OnlineUserTrackerService to support Guid-based user tracking
    - Add IsUserOnlineAsync(Guid userId) method
    - Add GetUserConnectionsAsync(Guid userId) method returning List<string>
    - Add GetOnlineUsersAsync() method returning List<Guid>
    - Update UserConnectionsCacheItem to include UserId (Guid), LastHeartbeat (DateTime), IsOnline (bool)
    - Keep existing phone number methods for backward compatibility
    - _Requirements: 2.1, 3.2_

  - [ ]* 4.2 Write property test for connection tracking
    - **Property 9: Connection State is Tracked**
    - **Validates: Requirements 2.1, 3.2**

  - [ ] 4.3 Implement heartbeat tracking
    - Add UpdateHeartbeatAsync(Guid userId) method
    - Add GetStaleConnectionsAsync(TimeSpan threshold) method
    - Add MarkUserOfflineAsync(Guid userId) method
    - _Requirements: 12.1, 12.2_

  - [ ] 4.4 Implement connection cleanup
    - Add CleanupStaleConnectionsAsync() method
    - Remove connections inactive for 5+ minutes
    - _Requirements: 12.4_

- [ ] 5. Enhance SignalR Hub for Message Delivery
  - [ ] 5.1 Update OnlineMobileUserHub connection handling
    - Update OnConnectedAsync to use Guid-based user tracking
    - Update OnDisconnectedAsync to use Guid-based user tracking
    - Trigger pending message delivery on connection using new GetPendingMessagesForUserAsync
    - Map CurrentUser.Id to Guid for user tracking
    - _Requirements: 2.1, 3.2_

  - [ ] 5.2 Enhance message acknowledgment methods
    - Update AcknowledgeMessage to set DeliveredAt timestamp
    - Add MarkMessageAsRead hub method
    - Update message status to "Read" and set ReadAt timestamp
    - Call Message entity methods (MarkAsDelivered, MarkAsRead)
    - _Requirements: 2.5, 4.5_

  - [ ]* 5.3 Write property tests for acknowledgment handling
    - **Property 8: Acknowledgment Updates Status to Delivered**
    - **Property 17: Read Status Updates Correctly**
    - **Property 29: Acknowledgment Prevents Redelivery**
    - **Validates: Requirements 1.9, 2.5, 4.5, 11.5**

  - [ ] 5.4 Implement Heartbeat hub method
    - Accept heartbeat pings from mobile clients
    - Update last heartbeat timestamp in OnlineUserTrackerService
    - _Requirements: 12.1_

- [ ] 6. Enhance Message Application Service
  - [ ] 6.1 Update SendOneWayMessageAsync endpoint
    - Update to use Guid-based SenderId and RecipientId
    - Add validation for recipient existence using MobileUser repository
    - Implement rate limiting (100 messages/minute per user) using IDistributedCache
    - Update to call enhanced MessageManager.CreateOneWayMessageAsync
    - Ensure message is persisted before publishing event
    - Return enhanced MessageDto with ID, status, and timestamps
    - _Requirements: 1.1, 1.4, 15.2_

  - [ ]* 6.2 Write property test for rate limiting
    - **Property 38: Rate Limiting Enforced Per User**
    - **Validates: Requirements 15.2**

  - [ ] 6.3 Implement message status query endpoint
    - Add GetMessageStatusAsync(Guid messageId) method
    - Return MessageDto with current status and all transition timestamps (SentAt, DeliveredAt, ReadAt)
    - _Requirements: 4.2_

  - [ ]* 6.4 Write property test for status query
    - **Property 14: Status Query Returns Complete History**
    - **Validates: Requirements 4.2**

  - [ ] 6.5 Implement dead letter queue management endpoints
    - Add GetDeadLetterQueueAsync() method returning PagedResultDto<MessageDto>
    - Add RetryFailedMessageAsync(Guid messageId) method
    - Reset retry count on manual retry
    - Republish SendOneWayMessageEto event
    - _Requirements: 13.2, 13.4, 13.5_

  - [ ]* 6.6 Write property tests for DLQ operations
    - **Property 33: DLQ Query Returns Complete Metadata**
    - **Property 34: Manual Retry Requeues Message**
    - **Property 35: Manual Retry Resets Retry Count**
    - **Validates: Requirements 13.3, 13.4, 13.5**

- [ ] 7. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 8. Enhance Message Delivery Handler
  - [ ] 8.1 Update MessageDeliveryHandler for distributed events
    - Update HandleEventAsync to use Guid-based RecipientId from SendOneWayMessageEto
    - Implement distributed locking using MessageManager.AcquireMessageLockAsync
    - Check if recipient is online using OnlineUserTrackerService.IsUserOnlineAsync(Guid)
    - Add proper lock release in finally block
    - _Requirements: 1.6, 1.7, 9.3_

  - [ ] 8.2 Enhance online user delivery logic
    - Update to deliver via SignalR to all user connections (not just first)
    - Call Message.MarkAsSent() and set SentAt timestamp
    - Implement acknowledgment timeout (30 seconds) using Task.Delay
    - Update message status in repository after delivery
    - _Requirements: 1.6, 2.2, 2.3, 2.4_

  - [ ]* 8.3 Write property tests for online delivery
    - **Property 6: Online Users Receive Real-Time Delivery**
    - **Property 10: Timeout Triggers Retry**
    - **Validates: Requirements 1.6, 2.3, 2.6**

  - [ ] 8.4 Update offline user handling logic
    - Keep message status as "Pending" (don't change from Queued)
    - Log info message about offline user with structured logging
    - _Requirements: 1.7, 3.1_

  - [ ]* 8.5 Write property test for offline handling
    - **Property 7: Offline Users Have Messages Queued**
    - **Validates: Requirements 1.7, 3.1**

  - [ ] 8.6 Implement delivery failure handling
    - Catch exceptions during delivery
    - Call Message.IncrementRetryCount()
    - Call RetryPolicyService.ScheduleRetryAsync (to be implemented in task 9)
    - Log errors with correlation IDs
    - _Requirements: 4.4, 5.1_

- [ ] 9. Implement Retry Policy Service
  - [ ] 9.1 Create RetryPolicyService domain service
    - Create in src/Esh3arTech.Domain/Messages/RetryPolicyService.cs
    - Implement ShouldRetry(Message message) method (max 5 retries)
    - Implement GetNextRetryDelay(int retryCount) with exponential backoff (1s, 2s, 4s, 8s, 16s)
    - Implement ScheduleRetryAsync(Message message) using IBackgroundJobManager
    - _Requirements: 5.1, 5.2_

  - [ ]* 9.2 Write property test for retry policy
    - **Property 18: Exponential Backoff Retry Policy**
    - **Property 19: Dead Letter Queue After Max Retries**
    - **Property 20: Retry Metadata is Persisted**
    - **Validates: Requirements 5.1, 5.2, 5.3, 5.4, 5.5**

  - [ ] 9.3 Implement retry background job
    - Create RetryMessageDeliveryArgs class
    - Create RetryMessageDeliveryJob inheriting from AsyncBackgroundJob<RetryMessageDeliveryArgs>
    - Republish SendOneWayMessageEto event for retry
    - Register job in module ConfigureServices
    - _Requirements: 5.1_

- [ ] 10. Implement Pending Messages Delivery Worker
  - [ ] 10.1 Create PendingMessagesDeliveryWorker background worker
    - Create in src/Esh3arTech.EntityFrameworkCore/BackgroundWorkers/
    - Extend AsyncPeriodicBackgroundWorkerBase
    - Set timer period to 5 seconds
    - Get list of online users from OnlineUserTrackerService.GetOnlineUsersAsync()
    - Register worker in Esh3arTechEntityFrameworkCoreModule
    - _Requirements: 3.2, 3.3_

  - [ ] 10.2 Implement pending message processing
    - For each online user, call MessageManager.GetPendingMessagesForUserAsync
    - Publish SendOneWayMessageEto events for pending messages
    - Messages already ordered chronologically by GetPendingMessagesForUserAsync
    - Add error handling and logging
    - _Requirements: 3.3, 3.4, 3.5_

  - [ ]* 10.3 Write property tests for pending message delivery
    - **Property 11: Pending Messages Retrieved on Reconnection**
    - **Property 12: Messages Delivered in Chronological Order**
    - **Property 13: Batch Status Update on Delivery**
    - **Property 31: Delivery Resumes from Last Acknowledged**
    - **Validates: Requirements 3.3, 3.4, 3.5, 12.5**

- [ ] 11. Implement Heartbeat Monitor Worker
  - [ ] 11.1 Create HeartbeatMonitorWorker background worker
    - Create in src/Esh3arTech.EntityFrameworkCore/BackgroundWorkers/
    - Extend AsyncPeriodicBackgroundWorkerBase
    - Set timer period to 30 seconds
    - Send heartbeat pings to all connected users via IHubContext<OnlineMobileUserHub>
    - Register worker in Esh3arTechEntityFrameworkCoreModule
    - _Requirements: 12.1_

  - [ ] 11.2 Implement stale connection detection
    - Call OnlineUserTrackerService.GetStaleConnectionsAsync(TimeSpan.FromSeconds(60))
    - For each stale user, call MarkUserOfflineAsync
    - Query messages with Status = "Sent" for offline users
    - Transition those messages back to "Pending" status
    - _Requirements: 12.2, 12.3_

  - [ ]* 11.3 Write property test for offline detection
    - **Property 30: Offline Marking Rolls Back Message Status**
    - **Validates: Requirements 12.3**

  - [ ] 11.4 Implement connection cleanup
    - Call OnlineUserTrackerService.CleanupStaleConnectionsAsync()
    - Clean up connections inactive for 5+ minutes
    - _Requirements: 12.4_

- [ ] 12. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 13. Implement Circuit Breaker Service
  - [ ] 13.1 Create CircuitBreakerService domain service
    - Create in src/Esh3arTech.Domain/Messages/CircuitBreakerService.cs
    - Create CircuitBreakerState cache item class
    - Implement IsOpenAsync(string resourceName) to check circuit state
    - Implement RecordSuccessAsync(string resourceName) and RecordFailureAsync(string resourceName)
    - Use IDistributedCache<CircuitBreakerState> for circuit state storage
    - _Requirements: 8.3_

  - [ ] 13.2 Implement circuit breaker logic
    - Calculate failure rate over sliding window
    - Open circuit when failure rate exceeds 50% with minimum 10 requests
    - Implement 60-second cooldown period
    - Implement TryCloseAsync(string resourceName) for recovery
    - _Requirements: 8.3, 8.4_

  - [ ]* 13.3 Write property test for circuit breaker
    - **Property 24: Circuit Breaker Opens at Threshold**
    - **Validates: Requirements 8.3**

  - [ ] 13.4 Integrate circuit breaker with delivery handler
    - Wrap SignalR hub calls with circuit breaker checks
    - Queue messages when circuit is open
    - Add circuit breaker resource names: "SignalR", "BlobStorage"
    - _Requirements: 8.4_

- [ ] 14. Implement Media File Handling
  - [ ] 14.1 Enhance message creation for media attachments
    - Update MessageManager.CreateOneWayMessageAsync to accept List<IFormFile> attachments
    - Validate media file types (JPEG, PNG, GIF, PDF, MP4, MOV)
    - Validate file sizes (max 10MB per file, max 5 files)
    - Upload files to ABP Blob Storage using IBlobContainer
    - Create MessageAttachment entities with blob names
    - _Requirements: 6.1, 6.2, 6.3_

  - [ ]* 14.2 Write property tests for media validation
    - **Property 3: Media Validation Rejects Invalid Files**
    - **Property 21: Media Upload Precedes Queuing**
    - **Validates: Requirements 6.1, 6.2, 6.3**

  - [ ] 14.3 Implement secure URL generation
    - Add GenerateAccessUrlAsync method to MessageManager
    - Generate time-limited access URLs for media files using blob storage
    - Set URL expiration to 24 hours
    - Store URLs in MessageAttachment.AccessUrl and UrlExpiresAt
    - _Requirements: 6.4_

  - [ ]* 14.4 Write property test for URL generation
    - **Property 22: Media URLs Generated with Expiration**
    - **Validates: Requirements 6.4**

  - [ ] 14.5 Include media URLs in message payload
    - Update MessageDto to include List<MessageAttachmentDto>
    - Update SendOneWayMessageEto to include attachment information
    - Include URLs in SignalR message delivery payload
    - _Requirements: 6.5_

  - [ ]* 14.6 Write property test for media payload
    - **Property 23: Message Payload Includes Media URLs**
    - **Validates: Requirements 6.5**

- [ ] 15. Implement Backpressure and Load Management
  - [ ] 15.1 Implement queue depth monitoring
    - Add GetQueueDepthAsync method to MessageManager
    - Query count of messages with Status = "Queued" or "Pending"
    - Implement threshold check (100,000 messages)
    - _Requirements: 15.1_

  - [ ] 15.2 Implement backpressure logic in MessageAppService
    - Check queue depth before accepting new messages in SendOneWayMessageAsync
    - Reject new message submissions when queue depth exceeds threshold
    - Return 503 Service Unavailable with descriptive message
    - Log warning with queue depth metrics
    - _Requirements: 15.1_

  - [ ]* 15.3 Write property test for backpressure
    - **Property 37: Backpressure Applied at Queue Threshold**
    - **Validates: Requirements 15.1**

  - [ ] 15.4 Enhance rate limiting implementation
    - Use IDistributedCache for rate limit counters with key pattern "RateLimit:{UserId}:{Minute}"
    - Track message count per user per minute
    - Return 429 Too Many Requests with Retry-After header when exceeded
    - Set cache TTL to 60 seconds
    - _Requirements: 15.2, 15.3_

  - [ ]* 15.5 Write unit test for rate limit response
    - Test HTTP 429 response with Retry-After header
    - _Requirements: 15.3_

- [ ] 16. Implement Monitoring and Logging
  - [ ] 16.1 Add structured logging for message operations
    - Add Serilog structured logging to MessageManager with correlation IDs
    - Log message creation with messageId, senderId, recipientId
    - Log all status transitions with timestamps and previous/new status
    - Log delivery attempts with success/failure indicators
    - Log retry operations with retry count and next retry time
    - _Requirements: 14.2_

  - [ ]* 16.2 Write property test for state transition logging
    - **Property 36: State Transitions Are Logged**
    - **Validates: Requirements 14.2**

  - [ ] 16.3 Implement dead letter queue logging
    - Log failure details in MessageManager.MoveToDeadLetterQueueAsync
    - Include error messages, retry history, and final failure reason
    - Use LogError level with structured data
    - _Requirements: 13.1_

  - [ ]* 16.4 Write property test for DLQ logging
    - **Property 32: Dead Letter Queue Logs Failure Details**
    - **Validates: Requirements 13.1**

  - [ ] 16.5 Add performance metrics (optional - for monitoring tools)
    - Document metrics to track: message throughput, delivery latency, queue depth, error rates, online user count
    - Add comments in code for future Application Insights integration
    - Log key performance indicators at Info level
    - _Requirements: 14.1_

- [ ] 17. Implement Additional Correctness Properties
  - [ ]* 17.1 Write property test for valid state transitions
    - **Property 15: Only Valid State Transitions Allowed**
    - **Validates: Requirements 4.3**

  - [ ]* 17.2 Write property test for failure tracking
    - **Property 16: Failure Tracking is Complete**
    - **Validates: Requirements 4.4**

  - [ ]* 17.3 Write property test for online user prioritization
    - **Property 27: Online Users Prioritized Over Offline**
    - **Validates: Requirements 10.4**

  - [ ]* 17.4 Write property test for idempotency
    - **Property 28: Idempotency Prevents Duplicate Delivery**
    - **Validates: Requirements 11.4**

- [ ] 18. Final Integration and Testing
  - [ ] 18.1 Create integration tests for end-to-end flows
    - Set up test project: test/Esh3arTech.Application.Tests (if not exists)
    - Test online user message delivery flow (create message → deliver → acknowledge)
    - Test offline user message queuing and delivery on reconnection
    - Test retry flow with simulated failures
    - Test dead letter queue flow after max retries
    - Test media attachment flow (if implemented)
    - _Requirements: All_

  - [ ] 18.2 Create load tests (optional - document approach)
    - Document approach for testing 10,000+ messages/second throughput
    - Document approach for testing latency under load (p95 < 200ms)
    - Document approach for testing horizontal scaling with multiple workers
    - Consider using tools like NBomber or k6 for load testing
    - _Requirements: 7.1, 10.1_

  - [ ] 18.3 Verify all property tests pass
    - Run all property tests with 100+ iterations
    - Verify all correctness properties are validated
    - Fix any failing tests
    - _Requirements: All_

- [ ] 19. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional property-based tests and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties with 100+ iterations using FsCheck
- Unit tests validate specific examples and edge cases
- Integration tests validate end-to-end flows
- The current codebase has basic messaging infrastructure; this plan adds guaranteed delivery features
- Use IDistributedCache (Redis) for connection state, distributed locking, and rate limiting
- Use IBackgroundJobManager for retry scheduling
- Use Serilog for structured logging with correlation IDs
