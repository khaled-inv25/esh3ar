using System;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.Messages.Buffer
{
    public class MessageBufferDto : FullAuditedEntityDto<Guid>
    {
        public string RecipientPhoneNumber { get; private set; }

        public string Subject { get; private set; }

        public string? MessageContent { get; private set; }

        public MessageStatus Status { get; private set; }

        public MessageType Type { get; private set; }
    }
}
