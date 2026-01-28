using System;

namespace Esh3arTech.Chats
{
    public class ReceiveToMobileMessageDto
    {
        public Guid SenderId { get; set; }

        public Guid MessageId { get; set; }

        public string ReceipientMobileNumber { get; set; }

        public string MobileAccount { get; set; }

        public string Content { get; set; }

    }
}
