using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace Esh3arTech.Messages
{
    [Table(Esh3arTechConsts.TblMessage)]
    public class Message : FullAuditedEntity<Guid>
    {
        public string RecipientPhoneNumber { get; set; }

        public string Subject { get; set; }

        public MessageContentType ContentType { get; set; }

        public string MessageContent { get; set; }

        public MessageStatus Status { get; set; }

        public Message(
            Guid id,
            string recipientPhoneNumber,
            string subject, 
            MessageContentType contentType, 
            string messageContent, 
            MessageStatus status)
            : base(id)
        {
            RecipientPhoneNumber = recipientPhoneNumber;
            Subject = subject;
            ContentType = contentType;
            MessageContent = messageContent;
            Status = status;
        }
    }
}
