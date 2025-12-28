# Implementation Plan: Reliable Message Delivery and High-Throughput Queuing

## Overview

This implementation plan transforms the existing messaging system into an enterprise-grade platform with reliable delivery and high-throughput capabilities. The plan is organized into phases to enable incremental delivery and testing.

## Tasks

- [ ] 1. Database Schema and Entity Enhancements
  - Add new properties to Message entity for retry tracking and idempotency
  - Create database migration
  - Add indexes for performance
  - _Requirements: 1.3, 1.6, 3.1, 3.5, 10.7_

- [ ] 1.1 Enhance Message entity with new properties
  - Add `IdempotencyKey` (string, required)
  - Add `NextRetryAt` (DateTime?, nullable)
  - Add `LastRetryAt` (DateTime?, nullable)
  - _Requirements: 1.3, 3.1, 3.5_

- [ ] 1.2 Add Message entity methods for retry logic
  - Implement `SetIdempotencyKey(string key)` method
  - Implement `IncrementRetryCount()` method
  - Implement `ScheduleNextRetry(TimeSpan delay)` method
  - Implement `CanRetry(int maxRetries)` method
  - Implement `MarkAsRetrying()` method
  - _Requirements: 1.1, 1.3, 1.4_

- [ ] 1.3 Create Entity Framework migration
  - Add columns: IdempotencyKey, NextRetryAt, LastRetryAt
  - Add index on `IdempotencyKey` for deduplication lookups
  - Add composite index on `(Status, NextRetryAt)` for retry worker queries
  - Add composite index on `(RecipientPhoneNumber, CreationTime)` for ordering
  - _Requirements: 1.3, 3.2, 10.1_

- [ ] 2. Retry Policy Implementation
  - Create retry policy service with exponential backoff
  - Configure retry limits and delays
  - _Requirements: 1.1, 1.2, 1.5, 1.6_

- [ ] 2.1 Create IMessageRetryPolicy interface
  - Define `CalculateDelay(int retryCount)` method
  - Define `CanRetry(int retryCount)` property
  - Define `MaxRetries` property
  - _Requirements: 1.1, 1.5_

- [ ] 2.2 Implement ExponentialBackoffRetryPolicy
  - Calculate delay as: min(baseDelay * 2^retryCount, maxDelay)
  - Support configurable base delay (default: 5 seconds)
  - Support configurable max delay (default: 300 seconds)
  - Support configurable max retries (default: 5)
  - _Requirements: 1.2, 1.5, 1.6_

- [ ] 2.3 Register retry policy in dependency injection
  - Register as singleton in Domain module
  - Bind configuration from appsettings.json
  - _Requirements: 1.5_

- [ ] 3. Idempotency Service Implementation
  - Create service to prevent duplicate message processing
  - Use distributed cache for tracking
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7_

- [ ] 3.1 Create IIdempotencyService interface
  - Define `IsProcessedAsync(string idempotencyKey)` method
  - Define `MarkAsProcessedAsync(string idempotencyKey, TimeSpan ttl)` method
  - Define `GenerateKeyAsync(Guid messageId)` method
  - _Requirements: 3.1, 3.2, 3.5_

- [ ] 3.2 Implement IdempotencyService
  - Use IDistributedCache for storage
  - Implement 24-hour TTL for deduplication records
  - Generate idempotency keys using message ID
  - Log duplicate detection attempts
  - _Requirements: 3.2, 3.3, 3.4, 3.6, 3.7_

- [ ] 3.3 Create IdempotencyRecord cache item class
  - Store message ID and processed timestamp
  - _Requirements: 3.4_

- [ ] 4. Circuit Breaker Implementation
  - Create circuit breaker to prevent cascading failures
  - Track failure rates and manage state transitions
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7, 4.8_

- [ ] 4.1 Create ICircuitBreaker interface
  - Define `IsOpenAsync()` method
  - Define `RecordSuccessAsync()` method
  - Define `RecordFailureAsync()` method
  - Define `GetState()` method
  - Define CircuitState enum (Closed, Open, HalfOpen)
  - _Requirements: 4.1, 4.2_

- [ ] 4.2 Implement MessageCircuitBreaker
  - Track failure rate over sliding window (default: 10 requests)
  - Open circuit when failure rate exceeds threshold (default: 50%)
  - Transition to half-open after timeout (default: 30 seconds)
  - Allow limited test requests in half-open state
  - Close circuit when test requests succeed
  - Use distributed cache for state persistence
  - _Requirements: 4.2, 4.3, 4.4, 4.5, 4.6, 4.7, 4.8_

- [ ] 4.3 Create CircuitBreakerState cache item class
  - Store state, failure count, last state change timestamp
  - _Requirements: 4.2_

- [ ] 5. Enhanced Message Delivery Handler
  - Integrate retry policy, idempotency, and circuit breaker
  - Add comprehensive error handling
  - _Requirements: 1.1, 1.7, 3.1, 3.3, 4.1, 4.4_

- [ ] 5.1 Enhance MessagesDeliveryHandler with idempotency checking
  - Check idempotency before processing
  - Skip processing if already processed
  - Mark as processed after successful delivery
  - _Requirements: 3.1, 3.3, 3.7_

- [ ] 5.2 Enhance MessagesDeliveryHandler with circuit breaker
  - Check circuit state before delivery attempt
  - Record success/failure after each attempt
  - Schedule retry if circuit is open
  - _Requirements: 4.1, 4.4_

- [ ] 5.3 Enhance MessagesDeliveryHandler with retry logic
  - Catch delivery failures
  - Increment retry count on failure
  - Calculate next retry delay using retry policy
  - Schedule next retry or move to DLQ if max retries exceeded
  - Update message status appropriately
  - _Requirements: 1.1, 1.3, 1.4, 1.7_

- [ ] 5.4 Add error classification logic
  - Classify errors as Transient, Recoverable, or Permanent
  - Move permanent errors to DLQ immediately
  - Apply retry logic only for transient and recoverable errors
  - _Requirements: 1.1, 1.4_

- [ ] 6. RabbitMQ Configuration Enhancements
  - Configure dead letter exchange and queue
  - Configure priority queues
  - _Requirements: 2.1, 2.2, 2.7, 5.1, 5.2, 5.3, 5.4_

- [ ] 6.1 Configure Dead Letter Exchange (DLX)
  - Create "esh3artech.messages.dlx" exchange
  - Create "esh3artech.messages.dlq" queue
  - Bind DLQ to DLX with routing key "failed"
  - Set message TTL to 24 hours in DLQ
  - _Requirements: 2.1, 2.2, 2.7_

- [ ] 6.2 Configure main queues with DLX routing
  - Add x-dead-letter-exchange argument to main queues
  - Add x-dead-letter-routing-key argument
  - Configure message TTL for retry delays
  - _Requirements: 2.1, 2.2_

- [ ] 6.3 Configure priority queues
  - Create "esh3artech.messages.high" queue with priority 10
  - Create "esh3artech.messages.normal" queue with priority 5
  - Create "esh3artech.messages.low" queue with priority 1
  - Configure routing based on message priority
  - _Requirements: 5.1, 5.2, 5.3, 5.4_

- [ ] 6.4 Update MessageAppService to route by priority
  - Route messages to appropriate queue based on Priority field
  - _Requirements: 5.1, 5.6_

- [ ] 7. Dead Letter Queue Management
  - Create service to manage failed messages
  - Provide API endpoints for DLQ operations
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7_

- [ ] 7.1 Create IDeadLetterQueueAppService interface
  - Define `GetDeadLetterMessagesAsync(PagedAndSortedResultRequestDto)` method
  - Define `RequeueMessageAsync(Guid messageId)` method
  - Define `RequeueMultipleMessagesAsync(List<Guid> messageIds)` method
  - Define `DeleteMessageAsync(Guid messageId)` method
  - _Requirements: 2.3, 2.4, 2.5_

- [ ] 7.2 Create DeadLetterMessageDto
  - Include Id, RecipientPhoneNumber, MessageContent, RetryCount, FailureReason, CreationTime, LastRetryAt
  - _Requirements: 2.3_

- [ ] 7.3 Implement DeadLetterQueueAppService
  - Query messages with Status = Failed
  - Implement requeue logic (reset RetryCount, update status to Pending)
  - Implement delete logic
  - Republish requeued messages to event bus
  - _Requirements: 2.3, 2.4, 2.5, 2.6_

- [ ] 8. Batch Message Processing
  - Create service for high-throughput bulk operations
  - Validate and queue messages in parallel
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.6, 6.7_

- [ ] 8.1 Create IBatchMessageProcessor interface
  - Define `SendBatchAsync(List<SendOneWayMessageDto>)` method
  - _Requirements: 6.1_

- [ ] 8.2 Create BatchSendMessageDto
  - List of SendOneWayMessageDto with max 1000 items
  - Add validation attributes
  - _Requirements: 6.1, 6.6_

- [ ] 8.3 Create BatchMessageResultDto
  - Include TotalMessages, SuccessCount, FailureCount
  - Include list of individual message results
  - _Requirements: 6.5_

- [ ] 8.4 Implement BatchMessageProcessor
  - Validate all messages before queuing any
  - Reject entire batch if any message fails validation
  - Queue messages in parallel using Task.WhenAll
  - Use EF Core bulk insert for efficiency
  - Return detailed batch results
  - _Requirements: 6.2, 6.3, 6.4, 6.5, 6.6, 6.7_

- [ ] 9. Message Acknowledgment Service
  - Create service for delivery confirmation
  - Track acknowledgment timeouts
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5, 7.6, 7.7_

- [ ] 9.1 Create IMessageAcknowledgmentService interface
  - Define `AcknowledgeDeliveryAsync(Guid messageId)` method
  - Define `AcknowledgeBatchAsync(List<Guid> messageIds)` method
  - _Requirements: 7.1, 7.4_

- [ ] 9.2 Create AcknowledgeMessageDto and AcknowledgeBatchDto
  - Single and batch acknowledgment DTOs
  - _Requirements: 7.1, 7.4_

- [ ] 9.3 Implement MessageAcknowledgmentService
  - Update message status to Delivered
  - Set DeliveredAt timestamp
  - Support bulk acknowledgment
  - Remove from pending delivery queue
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.7_

- [ ] 9.4 Add acknowledgment timeout tracking
  - Track messages not acknowledged within timeout (default: 5 minutes)
  - Trigger retry for timed-out messages
  - _Requirements: 7.5, 7.6_

- [ ] 10. Metrics Collection and Monitoring
  - Create service to track system metrics
  - Expose metrics via API endpoint
  - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 8.7, 8.8_

- [ ] 10.1 Create IMessageMetricsCollector interface
  - Define `RecordMessageProcessedAsync()` method
  - Define `RecordMessageFailedAsync()` method
  - Define `RecordProcessingTimeAsync(TimeSpan)` method
  - Define `GetMetricsAsync()` method
  - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5, 8.6_

- [ ] 10.2 Create MessageMetricsDto
  - Include MessagesPerSecond, AverageProcessingTimeMs, QueueDepth, RetryRate, FailureRate, CircuitBreakerState
  - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5, 8.6_

- [ ] 10.3 Implement MessageMetricsCollector
  - Use in-memory counters with sliding windows
  - Calculate rates and averages
  - Track queue depths by priority
  - Expose circuit breaker state
  - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5, 8.6_

- [ ] 10.4 Add metrics logging
  - Log metrics at configurable intervals (default: 1 minute)
  - _Requirements: 8.7_

- [ ] 10.5 Add metrics API endpoint
  - Create endpoint to expose current metrics
  - _Requirements: 8.6_

- [ ] 11. Background Worker for Retry Processing
  - Create worker to scan and requeue messages ready for retry
  - Run periodically to process scheduled retries
  - _Requirements: 1.1, 1.7_

- [ ] 11.1 Create MessageRetryWorker
  - Extend AsyncPeriodicBackgroundWorkerBase
  - Set timer period to 30 seconds
  - Query messages where Status = Failed AND NextRetryAt <= Now AND RetryCount < MaxRetries
  - Republish messages to event bus
  - Update message status to Queued
  - _Requirements: 1.1, 1.7_

- [ ] 11.2 Register worker in EntityFrameworkCore module
  - Add to background workers collection
  - _Requirements: 1.1_

- [ ] 12. Configuration and Settings
  - Add configuration section for reliability settings
  - Bind settings to services
  - _Requirements: 1.5, 1.6, 3.4, 4.2, 4.3, 4.6, 6.6, 7.5, 9.4_

- [ ] 12.1 Add MessageReliability configuration section
  - Add MaxRetries (default: 5)
  - Add BaseRetryDelaySeconds (default: 5)
  - Add MaxRetryDelaySeconds (default: 300)
  - Add IdempotencyTtlHours (default: 24)
  - Add CircuitBreakerFailureThreshold (default: 0.5)
  - Add CircuitBreakerSampleSize (default: 10)
  - Add CircuitBreakerTimeoutSeconds (default: 30)
  - Add AcknowledgmentTimeoutMinutes (default: 5)
  - Add BatchSizeLimit (default: 1000)
  - _Requirements: 1.5, 1.6, 3.4, 4.2, 4.3, 4.6, 6.6, 7.5_

- [ ] 12.2 Create MessageReliabilityOptions class
  - Map configuration to strongly-typed options
  - _Requirements: 1.5_

- [ ] 12.3 Configure options in Web module
  - Bind configuration section to options
  - _Requirements: 1.5_

- [ ] 13. Message Ordering Implementation
  - Implement FIFO ordering per recipient
  - Use recipient as partition key
  - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6, 10.7_

- [ ] 13.1 Add sequence number to Message entity
  - Add `SequenceNumber` property (long)
  - Generate sequence per recipient
  - _Requirements: 10.7_

- [ ] 13.2 Configure RabbitMQ routing for ordering
  - Use RecipientPhoneNumber as routing key
  - Ensure messages to same recipient go to same queue partition
  - _Requirements: 10.1, 10.2, 10.3_

- [ ] 13.3 Implement ordering logic in delivery handler
  - Process messages in sequence order per recipient
  - Block subsequent messages if earlier message fails
  - _Requirements: 10.1, 10.2, 10.4_

- [ ] 13.4 Add configurable ordering mode
  - Support strict FIFO or best-effort modes
  - _Requirements: 10.5, 10.6_

- [ ] 14. Graceful Degradation Features
  - Implement rate limiting and health checks
  - Handle queue unavailability gracefully
  - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6, 9.7_

- [ ] 14.1 Add rate limiting to MessageAppService
  - Implement configurable rate limit
  - Return 429 status with retry-after header when exceeded
  - Allow high-priority messages even when rate limited
  - _Requirements: 9.3, 9.4, 9.5_

- [ ] 14.2 Implement queue unavailability handling
  - Persist messages to database with Pending status when queue unavailable
  - Automatically requeue when queue becomes available
  - _Requirements: 9.1, 9.2_

- [ ] 14.3 Create health check endpoint
  - Check RabbitMQ connection status
  - Check database connection status
  - Check cache connection status
  - Return overall health status
  - _Requirements: 9.6_

- [ ] 14.4 Add error handling for database unavailability
  - Return appropriate error responses
  - Prevent data loss
  - _Requirements: 9.7_

- [ ] 15. Update SendOneWayMessageEto with idempotency
  - Add IdempotencyKey property to event
  - Generate key when creating message
  - _Requirements: 3.1, 3.5_

- [ ] 15.1 Add IdempotencyKey property to SendOneWayMessageEto
  - Add string property
  - _Requirements: 3.5_

- [ ] 15.2 Update MessageAppService to generate and set idempotency key
  - Generate key using IdempotencyService
  - Set on message entity and ETO
  - _Requirements: 3.1, 3.5_

- [ ] 16. Cleanup and Optimization
  - Remove dead code
  - Add logging and documentation
  - _Requirements: All_

- [ ] 16.1 Delete commented-out MessageManager.cs
  - Remove obsolete file
  - _Requirements: N/A_

- [ ] 16.2 Add comprehensive logging
  - Log all state transitions
  - Log retry attempts
  - Log circuit breaker state changes
  - Log idempotency hits
  - _Requirements: 3.7, 8.7_

- [ ] 16.3 Add XML documentation comments
  - Document all public interfaces and classes
  - _Requirements: All_

- [ ] 17. Testing and Validation
  - Write unit tests for core components
  - Write integration tests for end-to-end flows
  - _Requirements: All_

- [ ] 17.1 Write unit tests for ExponentialBackoffRetryPolicy
  - Test delay calculation
  - Test max retry enforcement
  - Test delay capping
  - _Requirements: 1.2, 1.5, 1.6_

- [ ] 17.2 Write unit tests for IdempotencyService
  - Test duplicate detection
  - Test TTL expiration
  - Test key generation
  - _Requirements: 3.1, 3.2, 3.4_

- [ ] 17.3 Write unit tests for MessageCircuitBreaker
  - Test state transitions
  - Test failure threshold detection
  - Test timeout and recovery
  - _Requirements: 4.2, 4.3, 4.5, 4.6, 4.7, 4.8_

- [ ] 17.4 Write integration tests for message delivery flow
  - Test successful delivery
  - Test retry on failure
  - Test move to DLQ after max retries
  - _Requirements: 1.1, 1.4, 2.1_

- [ ] 17.5 Write integration tests for batch processing
  - Test successful batch
  - Test batch rejection on validation failure
  - _Requirements: 6.2, 6.3_

- [ ] 17.6 Write integration tests for priority queues
  - Test high priority processed first
  - Test starvation prevention
  - _Requirements: 5.2, 5.3, 5.5_

- [ ] 18. Checkpoint - Ensure all tests pass
  - Run all unit tests
  - Run all integration tests
  - Verify metrics are being collected
  - Check logs for errors
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks are organized in dependency order - complete earlier tasks before later ones
- Each task references specific requirements for traceability
- Database migration (task 1.3) should be run before deploying code changes
- Configuration changes (task 12) should be deployed before enabling new features
- Testing tasks (17) should be executed throughout development, not just at the end
- The implementation is backward compatible - existing functionality continues to work
