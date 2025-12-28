namespace Esh3arTech.Messages
{
    public class MessageReliabilityOptions
    {
        public int MaxRetries { get; set; }

        public int BaseRetryDelaySeconds { get; set; }

        public int MaxRetryDelaySeconds { get; set; }

        public int IdempotencyTtlHours { get; set; }

        public double CircuitBreakerFailureThreshold { get; set; }

        public int CircuitBreakerSampleSize { get; set; }

        public int CircuitBreakerTimeoutSeconds { get; set; }

        public int AcknowledgmentTimeoutMinutes { get; set; }

        public int BatchSizeLimit { get; set; }

    }
}
