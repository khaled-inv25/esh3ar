using System;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.Messages
{
    public class MessageInListDto : EntityDto<Guid>
    {
        public string RecipientPhoneNumber { get; set; }

        public string MessageContent { get; set; }

        public string CreationTime { get; set; }

        public MessageStatus Status { get; set; }
    }
}
