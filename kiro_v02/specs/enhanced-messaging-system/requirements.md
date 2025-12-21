# Requirements Document: Enhanced Messaging System

## Introduction

This document specifies the requirements for enhancing the Esh3arTech messaging system to support 1-to-1 chat, media sharing, push notifications, reliable message delivery, and high-throughput message queuing. The enhancements will transform the current one-way notification system into a full-featured, scalable messaging platform with high availability and fault tolerance.

## Glossary

- **Message System**: The core messaging infrastructure that handles message creation, routing, delivery, and status tracking
- **One-Way Message**: A read-only message sent from a business user to mobile users, delivered in real-time if online or queued for delivery when user comes back online (e.g., announcements, alerts, updates)
- **Chat Message**: A bidirectional message sent between a business user and a mobile user within a conversation context where both participants can send and receive messages
- **Conversation**: A bidirectional message thread between a business user and a mobile user for chat purposes
- **Message Type**: An enumeration distinguishing between OneWay (read-only from business) and Chat (bidirectional between business and mobile user) messages
- **Read-Only Message**: A message that mobile users can view but cannot reply to directly
- **Media Attachment**: Files (images, videos, audio, documents) attached to messages, supported for both one-way and chat messages
- **Push Notification**: Mobile platform notifications (FCM for Android, APNS for iOS) sent when users are offline
- **Message Queue**: A buffer system that temporarily stores messages for asynchronous processing
- **Delivery Acknowledgment**: Confirmation that a message was successfully received by the recipient for both one-way and bidirectional
- **Backplane**: A distributed messaging infrastructure (Redis) that enables SignalR to work across multiple server instances
- **Circuit Breaker**: A fault tolerance pattern that prevents cascading failures by temporarily blocking requests to failing services
- **Idempotency**: The property that ensures duplicate message processing produces the same result as single processing
- **Dead Letter Queue**: A storage location for messages that cannot be delivered after multiple retry attempts

## Requirements

### Requirement 1: Dual-Mode Messaging (One-Way Read-Only and 1-to-1 Chat)

**User Story:** As a system user, I want to support both one-way read-only messages from businesses to mobile users and bidirectional chat conversations between mobile users, so that the platform can serve multiple communication use cases.

#### Acceptance Criteria

1. WHEN a business user sends a one-way message THEN the Message System SHALL create a read-only message without requiring a conversation context
2. WHEN a one-way message is sent to an online mobile user THEN the Message System SHALL deliver the message in real-time via SignalR
3. WHEN a one-way message is sent to an offline mobile user THEN the Message System SHALL queue the message and deliver it when the user comes back online
4. WHEN a mobile user receives a one-way message THEN the Message System SHALL display the message as read-only without reply capability
5. WHEN a mobile user sends a message to a business user THEN the Message System SHALL create a chat message within a conversation context
6. WHEN a business user and a mobile user exchange messages THEN the Message System SHALL group messages into a conversation thread identified by a unique conversation ID
7. WHEN a user requests conversation history THEN the Message System SHALL return only chat messages in chronological order with pagination support
8. WHEN a user requests one-way message history THEN the Message System SHALL return only read-only messages in chronological order with pagination support
9. WHEN creating a chat message THEN the Message System SHALL validate that the mobile user is verified and the business user has chat permissions
10. WHEN creating a one-way message THEN the Message System SHALL validate that the sender has message sending permissions
11. WHEN a conversation is created THEN the Message System SHALL assign a unique identifier and track both participants (one business user and one mobile user)
12. WHEN retrieving messages THEN the Message System SHALL distinguish between one-way messages and chat messages using a message type field
13. WHEN a business user views conversations THEN the Message System SHALL show all conversations with mobile users they are chatting with
14. WHEN a mobile user views conversations THEN the Message System SHALL show all conversations with business users they are chatting with
15. WHEN a mobile user receives a one-way message THEN the Message System SHALL track the message status and updated as Delivered.

### Requirement 2: Media Sharing

**User Story:** As a user, I want to share images, videos, audio files, and documents in my messages, so that I can communicate with rich content beyond text.

#### Acceptance Criteria

1. WHEN a user uploads a media file THEN the Message System SHALL validate the file type against allowed types (image/*, video/*, audio/*, application/pdf, application/msword)
2. WHEN a media file is uploaded THEN the Message System SHALL validate the file size does not exceed the maximum limit (50MB for videos, 10MB for images, 5MB for documents)
3. WHEN a valid media file is uploaded THEN the Message System SHALL store the file in blob storage and generate a unique file identifier
4. WHEN an image is uploaded THEN the Message System SHALL generate thumbnail versions (small: 150x150, medium: 300x300, large: 600x600)
5. WHEN a media file is stored THEN the Message System SHALL create a media attachment record linked to the message with metadata (filename, size, content type, storage URL)
6. WHEN a user requests a media file THEN the Message System SHALL generate a time-limited signed URL valid for 1 hour
7. WHEN a message contains media THEN the Message System SHALL include media attachment references in the message payload

### Requirement 3: Push Notifications

**User Story:** As a mobile user, I want to receive push notifications when I receive messages while offline, so that I stay informed even when not actively using the app.

#### Acceptance Criteria

1. WHEN a mobile user registers a device THEN the Message System SHALL store the device token (FCM for Android, APNS for iOS) associated with the user
2. WHEN a message is sent to an offline user THEN the Message System SHALL send a push notification to all registered devices for that user
3. WHEN a push notification is sent THEN the Message System SHALL include message preview (sender name, first 100 characters of content) and conversation ID
4. WHEN a user has multiple devices THEN the Message System SHALL send push notifications to all active devices
5. WHEN a push notification fails to deliver THEN the Message System SHALL log the failure and mark the device token as potentially invalid after 3 consecutive failures
6. WHEN a user disables notifications THEN the Message System SHALL respect the user preference and not send push notifications
7. WHEN a push notification is successfully delivered THEN the Message System SHALL track the delivery status

### Requirement 4: Reliable Message Delivery

**User Story:** As a system administrator, I want guaranteed message delivery with retry mechanisms, so that messages are not lost due to temporary failures.

#### Acceptance Criteria

1. WHEN a message delivery fails THEN the Message System SHALL retry delivery using exponential backoff (1s, 2s, 4s, 8s, 16s, 32s)
2. WHEN a message fails after maximum retry attempts (6 attempts) THEN the Message System SHALL move the message to a dead letter queue
3. WHEN a message is processed THEN the Message System SHALL use idempotency keys to prevent duplicate processing
4. WHEN a recipient acknowledges message receipt THEN the Message System SHALL update the message status to Delivered and stop retry attempts
5. WHEN a message is in the dead letter queue THEN the Message System SHALL notify administrators and allow manual reprocessing
6. WHEN the Message System processes a message THEN the Message System SHALL ensure at-least-once delivery semantics
7. WHEN a message delivery is attempted THEN the Message System SHALL log the attempt with timestamp, status, and error details if applicable

### Requirement 5: High-Throughput Message Queuing

**User Story:** As a system architect, I want asynchronous message processing with high throughput, so that the system can handle large volumes of concurrent messages efficiently.

#### Acceptance Criteria

1. WHEN a message is created THEN the Message System SHALL enqueue the message for asynchronous processing rather than processing synchronously
2. WHEN messages are enqueued THEN the Message System SHALL support priority levels (High, Normal, Low) with high-priority messages processed first
3. WHEN processing messages THEN the Message System SHALL process messages in batches of up to 100 messages per batch
4. WHEN the queue depth exceeds 10,000 messages THEN the Message System SHALL apply rate limiting to prevent queue overflow
5. WHEN a user sends messages THEN the Message System SHALL enforce per-user rate limits (100 messages per minute)
6. WHEN messages are queued THEN the Message System SHALL partition messages by conversation ID to enable parallel processing
7. WHEN the system is under load THEN the Message System SHALL maintain message processing latency below 500ms for 95th percentile

### Requirement 6: High Availability and Fault Tolerance

**User Story:** As a system administrator, I want the messaging system to remain available during failures, so that users experience minimal disruption.

#### Acceptance Criteria

1. WHEN a server instance fails THEN the Message System SHALL automatically route traffic to healthy instances without message loss
2. WHEN the database connection fails THEN the Message System SHALL implement circuit breaker pattern to prevent cascading failures
3. WHEN external services are unavailable THEN the Message System SHALL degrade gracefully and queue operations for later retry
4. WHEN the system starts up THEN the Message System SHALL perform health checks on all dependencies (database, Redis, RabbitMQ, blob storage)
5. WHEN health checks fail THEN the Message System SHALL report unhealthy status to load balancers and stop accepting new requests
6. WHEN shutting down THEN the Message System SHALL gracefully complete in-flight message processing before terminating
7. WHEN a component fails THEN the Message System SHALL isolate the failure using bulkhead pattern to prevent affecting other components

### Requirement 7: Horizontal Scalability

**User Story:** As a system architect, I want to scale the messaging system horizontally, so that it can handle growing user loads by adding more server instances.

#### Acceptance Criteria

1. WHEN multiple server instances are deployed THEN the Message System SHALL use Redis backplane for SignalR to enable cross-server real-time communication
2. WHEN tracking online users THEN the Message System SHALL store connection information in distributed Redis cache rather than in-memory
3. WHEN a user connects THEN the Message System SHALL register the connection in Redis with automatic expiration after 5 minutes of inactivity
4. WHEN routing messages THEN the Message System SHALL use consistent hashing to distribute load evenly across server instances
5. WHEN scaling up THEN the Message System SHALL support adding new server instances without downtime or message loss
6. WHEN scaling down THEN the Message System SHALL drain connections gracefully before removing instances
7. WHEN processing messages THEN the Message System SHALL use stateless design to enable any instance to process any message

### Requirement 8: Low Latency Performance

**User Story:** As a user, I want fast message delivery and retrieval, so that conversations feel responsive and real-time.

#### Acceptance Criteria

1. WHEN retrieving recent messages THEN the Message System SHALL cache the last 50 messages per conversation in Redis with 5-minute TTL
2. WHEN a user requests conversation list THEN the Message System SHALL return results within 200ms for 95th percentile
3. WHEN sending a message THEN the Message System SHALL acknowledge receipt within 100ms and process asynchronously
4. WHEN retrieving media files THEN the Message System SHALL serve files from CDN with edge caching
5. WHEN querying message history THEN the Message System SHALL use database indexes on (ConversationId, CreatedAt) for efficient retrieval
6. WHEN loading conversations THEN the Message System SHALL implement pagination with cursor-based navigation for consistent performance
7. WHEN the cache is unavailable THEN the Message System SHALL fall back to database queries without failing requests

### Requirement 9: Message Delivery Status Tracking

**User Story:** As a user, I want to see the delivery status of my messages (sent, delivered, read), so that I know when recipients have received and read my messages.

#### Acceptance Criteria

1. WHEN a message is created THEN the Message System SHALL set the initial status to Pending
2. WHEN a message is successfully enqueued THEN the Message System SHALL update the status to Sent
3. WHEN a recipient's device receives the message THEN the Message System SHALL update the status to Delivered
4. WHEN a recipient opens and views the message THEN the Message System SHALL update the status to Read
5. WHEN a message fails delivery THEN the Message System SHALL update the status to Failed with error details
6. WHEN a message status changes THEN the Message System SHALL notify the sender via SignalR if online
7. WHEN retrieving messages THEN the Message System SHALL include the current status and timestamp of last status change

### Requirement 10: Data Consistency and Integrity

**User Story:** As a system administrator, I want data consistency guarantees, so that message data remains accurate and reliable across distributed components.

#### Acceptance Criteria

1. WHEN creating a message THEN the Message System SHALL use database transactions to ensure atomic creation of message and related records
2. WHEN updating message status THEN the Message System SHALL use optimistic concurrency control to prevent conflicting updates
3. WHEN processing duplicate messages THEN the Message System SHALL detect duplicates using message ID and skip reprocessing
4. WHEN synchronizing cache and database THEN the Message System SHALL invalidate cache entries on database updates
5. WHEN a distributed transaction spans multiple services THEN the Message System SHALL implement saga pattern for eventual consistency
6. WHEN data conflicts occur THEN the Message System SHALL log conflicts and apply last-write-wins resolution strategy
7. WHEN querying data THEN the Message System SHALL use read-your-writes consistency to ensure users see their own updates immediately
