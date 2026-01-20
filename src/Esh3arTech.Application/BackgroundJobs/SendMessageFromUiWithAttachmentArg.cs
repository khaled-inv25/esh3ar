using Esh3arTech.Messages;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.BackgroundJobs
{
    public class SendMessageFromUiWithAttachmentArg : AuditedEntityDto<Guid>
    {
        public string RecipientPhoneNumber { get; set; }

        public string Subject { get; set; }

        public string? MessageContent { get; set; }

        public ICollection<MessageAttachment> Attachments { get; set; } = [];

        //public Message Arg {  get; set; }
    }
}
