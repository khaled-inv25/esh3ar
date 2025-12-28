using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Messages.Metrics
{
    public interface IMessageMetricsCollector : ITransientDependency
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
        public CircuitBreaker.CircuitState CircuitBreakerState { get; set; }
    }
}

