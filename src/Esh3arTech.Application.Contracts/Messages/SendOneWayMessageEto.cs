using System;
using Volo.Abp.EventBus;

namespace Esh3arTech.Messages
{
    [EventName("Esh3arTech.Messages.SendMessageEto")]
    public class SendOneWayMessageEto : MessageBaseDto
    {
        public Guid Id { get; set; }

        public string From { get; set; }

        public string Subject { get; set; }

        public MessageStatus Status { get; private set; }

        public MessageType Type { get; private set; }

        public Priority Priority { get; private set; }
    }
}
