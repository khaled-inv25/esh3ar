# Requirements Document

## Introduction

This specification defines requirements for implementing reliable message delivery and high-throughput message queuing in the Esh3arTech messaging platform. The current system uses RabbitMQ for basic event distribution but lacks critical reliability features such as automatic retry with exponential backoff, dead letter handling, message deduplication, delivery guarantees, and throughput optimization.

## Glossary

- **Message_Queue**: The RabbitMQ-based distributed queue system that handles message processing
- **Message_Entity**: The domain entity representing a message in the database
- **Retry_Policy**: The strategy for retrying failed message deliveries with exponential backoff
- **Dead_Letter_Queue**: A separate queue for messages that have exceeded maximum retry attempts
- **Idempotency_Key**: A unique identifier used to prevent duplicate message processing
- **Circuit_Breaker**: A pattern that prevents cascading failures by temporarily stopping requests to failing services
- **Batch_Processor**: A component that processes multiple messages in a single operation
- **Message_Status_Updater**: The service responsible for updating message delivery status

## Requirements

### Requirement 1: Automatic Retry with Exponential Backoff

**User Story:** As a system administrator, I want failed messages to be automatically retried with increasing delays, so that temporary failures don't result in lost messages.

#### Acceptance Criteria

1. WHEN a message delivery fails, THE Message_Queue SHALL automatically retry the delivery ✅
2. WHEN retrying a message, THE Retry_Policy SHALL apply exponential backoff with configurable base delay and maximum delay ✅
3. WHEN a message is retried, THE Message_Entity SHALL increment its RetryCount field ✅
4. WHEN a message reaches the maximum retry count, THE Message_Queue SHALL move it to the Dead_Letter_Queue ✅
5. THE Retry_Policy SHALL support configurable maximum retry attempts (default: 5 attempts) ✅
6. THE Retry_Policy SHALL calculate delay as: min(base_delay \* 2^retry_count, max_delay) ✅
7. WHEN a retry succeeds, THE Message_Status_Updater SHALL update the message status to Delivered ✅

### Requirement 2: Dead Letter Queue Management

**User Story:** As a system administrator, I want messages that fail after all retries to be stored in a dead letter queue, so that I can investigate and manually reprocess them.

#### Acceptance Criteria

1. WHEN a message exceeds maximum retry attempts, THE Message_Queue SHALL move it to the Dead_Letter_Queue
2. WHEN a message is moved to Dead_Letter_Queue, THE Message_Entity SHALL set status to Failed and record FailureReason
3. THE System SHALL provide an API endpoint to retrieve messages from the Dead_Letter_Queue
4. THE System SHALL provide an API endpoint to manually requeue messages from the Dead_Letter_Queue
5. THE System SHALL provide an API endpoint to permanently delete messages from the Dead_Letter_Queue
6. WHEN requeuing a message, THE System SHALL reset the RetryCount to zero
7. THE Dead_Letter_Queue SHALL persist messages until explicitly removed

### Requirement 3: Message Deduplication

**User Story:** As a system administrator, I want to prevent duplicate message processing, so that recipients don't receive the same message multiple times.

#### Acceptance Criteria

1. WHEN creating a message, THE System SHALL generate a unique Idempotency_Key
2. WHEN processing a message, THE Message_Queue SHALL check if the Idempotency_Key has been processed recently
3. IF an Idempotency_Key has been processed within the deduplication window, THEN THE Message_Queue SHALL skip processing and acknowledge the message
4. THE System SHALL maintain a deduplication cache with configurable time-to-live (default: 24 hours)
5. THE Idempotency_Key SHALL be stored in the Message_Entity
6. THE System SHALL use a distributed cache (Redis or in-memory) for deduplication tracking
7. WHEN a duplicate is detected, THE System SHALL log the duplicate attempt with message ID and timestamp

### Requirement 4: Circuit Breaker Pattern

**User Story:** As a system administrator, I want the system to temporarily stop sending messages when the external service is down, so that we don't waste resources on requests that will fail.

#### Acceptance Criteria

1. THE System SHALL implement a Circuit_Breaker for external message delivery services
2. WHEN the failure rate exceeds a threshold (default: 50% over 10 requests), THE Circuit_Breaker SHALL open
3. WHILE the Circuit_Breaker is open, THE System SHALL immediately fail message delivery attempts without calling the external service
4. WHEN the Circuit_Breaker is open, THE Message_Queue SHALL queue messages for retry when the circuit closes
5. THE Circuit_Breaker SHALL transition to half-open state after a configurable timeout (default: 30 seconds)
6. WHILE in half-open state, THE Circuit_Breaker SHALL allow a limited number of test requests
7. IF test requests succeed, THEN THE Circuit_Breaker SHALL close and resume normal operation
8. IF test requests fail, THEN THE Circuit_Breaker SHALL reopen and reset the timeout

### Requirement 5: Priority Queue Processing

**User Story:** As a system administrator, I want high-priority messages to be processed before normal-priority messages, so that urgent communications are delivered first.

#### Acceptance Criteria

1. THE Message_Queue SHALL support multiple priority levels (High, Normal, Low)
2. WHEN messages are queued, THE System SHALL route them to priority-specific queues
3. THE Message_Queue SHALL process high-priority messages before normal-priority messages
4. THE Message_Queue SHALL process normal-priority messages before low-priority messages
5. THE System SHALL prevent starvation by processing at least one lower-priority message for every N high-priority messages (configurable, default: N=10)
6. WHEN creating a message, THE System SHALL allow specifying priority (default: Normal)
7. THE Message_Entity Priority field SHALL be used to determine queue routing

### Requirement 6: Batch Message Processing

**User Story:** As a system administrator, I want to process multiple messages in batches, so that we can achieve higher throughput for bulk operations.

#### Acceptance Criteria

1. THE System SHALL provide an API endpoint to submit multiple messages in a single request
2. WHEN batch processing, THE Batch_Processor SHALL validate all messages before queuing any
3. IF any message in a batch fails validation, THEN THE System SHALL reject the entire batch
4. THE Batch_Processor SHALL queue messages in parallel to maximize throughput
5. THE System SHALL return a batch result containing the status of each message
6. THE Batch_Processor SHALL support configurable batch size limits (default: 1000 messages)
7. WHEN batch processing, THE System SHALL use database bulk insert operations for efficiency

### Requirement 7: Message Delivery Acknowledgment

**User Story:** As a mobile user, I want to acknowledge message receipt, so that the system knows I received the message successfully.

#### Acceptance Criteria

1. THE System SHALL provide an API endpoint for mobile clients to acknowledge message delivery
2. WHEN a mobile client acknowledges delivery, THE Message_Status_Updater SHALL update status to Delivered
3. WHEN a mobile client acknowledges delivery, THE Message_Entity SHALL set DeliveredAt timestamp
4. THE System SHALL support bulk acknowledgment of multiple messages
5. IF a message is not acknowledged within a timeout period (configurable, default: 5 minutes), THE System SHALL retry delivery
6. THE System SHALL track acknowledgment attempts separately from delivery attempts
7. WHEN a message is acknowledged, THE Message_Queue SHALL remove it from the pending delivery queue

### Requirement 8: Throughput Monitoring and Metrics

**User Story:** As a system administrator, I want to monitor message throughput and queue health, so that I can identify bottlenecks and scale appropriately.

#### Acceptance Criteria

1. THE System SHALL track messages processed per second
2. THE System SHALL track average message processing time
3. THE System SHALL track queue depth for each priority level
4. THE System SHALL track retry rate and failure rate
5. THE System SHALL track circuit breaker state changes
6. THE System SHALL expose metrics via a monitoring endpoint
7. THE System SHALL log performance metrics at configurable intervals (default: 1 minute)
8. THE System SHALL alert when queue depth exceeds threshold (configurable)

### Requirement 9: Graceful Degradation

**User Story:** As a system administrator, I want the system to continue accepting messages even when processing is degraded, so that we don't lose messages during outages.

#### Acceptance Criteria

1. WHEN the Message_Queue is unavailable, THE System SHALL persist messages to the database with Pending status
2. WHEN the Message_Queue becomes available, THE System SHALL automatically requeue pending messages
3. THE System SHALL implement rate limiting to prevent queue overflow (configurable limit)
4. WHEN rate limit is exceeded, THE System SHALL return a 429 status code with retry-after header
5. THE System SHALL continue accepting high-priority messages even when rate limited
6. THE System SHALL provide a health check endpoint indicating queue availability
7. WHEN database is unavailable, THE System SHALL return appropriate error responses without data loss

### Requirement 10: Message Ordering Guarantees

**User Story:** As a system administrator, I want messages to the same recipient to be delivered in order, so that conversation context is preserved.

#### Acceptance Criteria

1. THE Message_Queue SHALL guarantee FIFO ordering for messages to the same recipient
2. WHEN multiple messages are sent to the same recipient, THE System SHALL process them in creation order
3. THE System SHALL use recipient phone number as the partition key for ordering
4. IF a message to a recipient fails, THE System SHALL block subsequent messages to that recipient until retry succeeds or message is moved to Dead_Letter_Queue
5. THE System SHALL support configurable ordering mode (strict FIFO or best-effort)
6. WHEN in best-effort mode, THE System SHALL allow parallel processing of messages to different recipients
7. THE System SHALL track message sequence numbers per recipient for ordering verification
