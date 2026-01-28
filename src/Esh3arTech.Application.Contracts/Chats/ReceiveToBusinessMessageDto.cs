using System;

namespace Esh3arTech.Chats
{
    public class ReceiveToBusinessMessageDto
    {
        public Guid Id { get; set; }

        public string From { get; set; }

        public string MobileAccount { get; set; }

        public string Content { get; set; }
    }
}
