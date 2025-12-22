using System;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.Messages
{
    public class PendingMessageDto : EntityDto<Guid>
    {
        public string RecipientPhoneNumber { get; set; }
        public string MessageContent { get; set; }
        public string From { get; set; }
    }
}
