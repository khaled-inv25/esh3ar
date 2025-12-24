using System;
using Volo.Abp.EventBus;

namespace Esh3arTech.Messages.Eto
{
    [EventName("Esh3arTech.Messages.SendMessageEto")]
    public class SendOneWayMessageEto : MessageBaseDto
    {
        public Guid Id { get; set; }

        public string From { get; set; }

        public string Subject { get; set; }

        public string AccessUrl { get; set; }

        public DateTime? UrlExpiresAt { get; set; }

    }
}
