using System;

namespace Esh3arTech.Messages
{
    public class UpdateMessageStatusDto
    {
        public Guid Id { get; set; }

        public MessageStatus Status { get; set; }
    }
}
