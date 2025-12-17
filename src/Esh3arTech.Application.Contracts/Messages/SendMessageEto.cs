using System;
using Volo.Abp.EventBus;

namespace Esh3arTech.Messages
{
    [EventName("Esh3arTech.Messages.SendMessageEto")]
    public class SendMessageEto : MessageBaseDto
    {
        public Guid Id { get; set; }

        public string From { get; set; }
    }
}
