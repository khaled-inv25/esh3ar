using System;

namespace Esh3arTech.Messages
{
    public class DeadLetterMessageDto
    {
        public Guid Id { get; set; }
        public string RecipientPhoneNumber { get; set; }
        public string? MessageContent { get; set; }
        public int RetryCount { get; set; }
        public string? FailureReason { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastRetryAt { get; set; }
    }
}

