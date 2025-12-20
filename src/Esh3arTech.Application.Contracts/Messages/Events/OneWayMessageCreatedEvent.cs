namespace Esh3arTech.Messages.Events
{
    public class OneWayMessageCreatedEvent : MessageBaseDto
    {
        public string From { get; set; }

        public string Subject { get; set; }

        public MessageStatus Status { get; private set; }

        public MessageType Type { get; private set; }

        public Priority Priority { get; private set; }

    }
}
