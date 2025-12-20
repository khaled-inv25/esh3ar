using System;

namespace Esh3arTech.Messages
{
    public record DeliverOneWayMessageDto()
    {
        public Guid Id { get; set; }
        public string From { get; set; }
        public string RecipientPhoneNumber { get; set; }
        public string Subject { get; set; }
        public string MessageContent { get; set; }
        public MessageType Type { get; set; }
        public Priority Priority { get; set; }
    }
}
