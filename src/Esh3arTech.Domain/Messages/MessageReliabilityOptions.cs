namespace Esh3arTech.Messages
{
    public class MessageReliabilityOptions
    {
        public int MaxRetryAttempts { get; set; }
        public int BaseRetryDelaySeconds { get; set; }
        public int MaxRetryDelaySeconds { get; set; }
    }
}
