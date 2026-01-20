using System;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.Messages
{
    public class EnqueueBatchMessageDto : FullAuditedEntityDto<Guid>
    {
        public string RecipientPhoneNumber { get; set; }

        public string Subject { get; set; }

        public string? MessageContent { get; set; }
    }
}
