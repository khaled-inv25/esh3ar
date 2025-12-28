using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Esh3arTech.Messages.CircuitBreaker;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Messages.Metrics
{
    public class MessageMetricsCollector : IMessageMetricsCollector, ITransientDependency
    {
        private readonly ICircuitBreaker _circuitBreaker;
        private readonly ConcurrentQueue<DateTime> _processedMessages = new();
        private readonly ConcurrentQueue<DateTime> _failedMessages = new();
        private readonly ConcurrentQueue<TimeSpan> _processingTimes = new();
        private readonly ConcurrentQueue<DateTime> _retryMessages = new();
        private DateTime _lastCleanup = DateTime.UtcNow;
        private const int MaxSamples = 1000;
        private const int CleanupIntervalMinutes = 5;

        public MessageMetricsCollector(ICircuitBreaker circuitBreaker)
        {
            _circuitBreaker = circuitBreaker;
        }

        public Task RecordMessageProcessedAsync()
        {
            _processedMessages.Enqueue(DateTime.UtcNow);
            CleanupIfNeeded();
            return Task.CompletedTask;
        }

        public Task RecordMessageFailedAsync()
        {
            _failedMessages.Enqueue(DateTime.UtcNow);
            CleanupIfNeeded();
            return Task.CompletedTask;
        }

        public Task RecordProcessingTimeAsync(TimeSpan duration)
        {
            _processingTimes.Enqueue(duration);
            CleanupIfNeeded();
            return Task.CompletedTask;
        }

        public Task<MessageMetricsDto> GetMetricsAsync()
        {
            var now = DateTime.UtcNow;
            var oneMinuteAgo = now.AddMinutes(-1);

            // Calculate messages per second (based on last minute)
            var recentProcessed = _processedMessages.Count(m => m >= oneMinuteAgo);
            var messagesPerSecond = recentProcessed / 60.0;

            // Calculate average processing time
            var avgProcessingTime = _processingTimes.Count > 0
                ? _processingTimes.Average(t => t.TotalMilliseconds)
                : 0;

            // Calculate retry rate
            var totalProcessed = _processedMessages.Count;
            var totalRetries = _retryMessages.Count;
            var retryRate = totalProcessed > 0 ? (double)totalRetries / totalProcessed : 0;

            // Calculate failure rate
            var totalFailed = _failedMessages.Count;
            var failureRate = totalProcessed > 0 ? (double)totalFailed / totalProcessed : 0;

            return Task.FromResult(new MessageMetricsDto
            {
                MessagesPerSecond = (long)messagesPerSecond,
                AverageProcessingTimeMs = avgProcessingTime,
                QueueDepth = 0, // Would need RabbitMQ connection to get actual queue depth
                RetryRate = retryRate,
                FailureRate = failureRate,
                CircuitBreakerState = _circuitBreaker.GetState()
            });
        }

        private void CleanupIfNeeded()
        {
            var now = DateTime.UtcNow;
            if ((now - _lastCleanup).TotalMinutes < CleanupIntervalMinutes)
                return;

            _lastCleanup = now;
            var cutoff = now.AddMinutes(-10); // Keep last 10 minutes of data

            // Cleanup old entries
            while (_processedMessages.TryPeek(out var timestamp) && timestamp < cutoff)
            {
                _processedMessages.TryDequeue(out _);
            }

            while (_failedMessages.TryPeek(out var timestamp) && timestamp < cutoff)
            {
                _failedMessages.TryDequeue(out _);
            }

            while (_retryMessages.TryPeek(out var timestamp) && timestamp < cutoff)
            {
                _retryMessages.TryDequeue(out _);
            }

            // Limit processing times to max samples
            while (_processingTimes.Count > MaxSamples)
            {
                _processingTimes.TryDequeue(out _);
            }
        }
    }
}

