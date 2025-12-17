using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.RegularExpressions;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Validation;

namespace Esh3arTech.Messages
{
    [Table(Esh3arTechConsts.TblMessage)]
    public class Message : FullAuditedEntity<Guid>
    {
        public string RecipientPhoneNumber { get; private set; }

        public string Subject { get; private set; }

        public MessageContentType ContentType { get; private set; }

        public string MessageContent { get; private set; }

        public MessageStatus Status { get; private set; }

        public Message(
            Guid id,
            string recipientPhoneNumber,
            string subject)
            : base(id)
        {
            SetRecipientPhoneNumber(recipientPhoneNumber);
            SetSubject(subject);
        }

        public Message SetRecipientPhoneNumber(string number)
        {
            RecipientPhoneNumber = Check.NotNullOrWhiteSpace(number, nameof(number));

            if (!Regex.IsMatch(number, @"^\d+$"))
                throw new BusinessException("Phone number must be digits only");

            RecipientPhoneNumber = number;

            return this;
        }

        public Message SetSubject(string subject)
        {
            Subject = Check.NotNullOrWhiteSpace(subject, nameof(subject));

            return this;
        }

        public Message SetContentType(MessageContentType contentType)
        {
            if (!Enum.IsDefined(typeof(MessageContentType), contentType))
            {
                throw new AbpValidationException();
            }

            ContentType = contentType;

            return this;
        }

        public Message SetMessageContent(string content)
        {
            MessageContent = Check.NotNullOrWhiteSpace(content, nameof(content));

            return this;
        }

        public Message SetMessageStatusType(MessageStatus status)
        {
            if (!Enum.IsDefined(typeof(MessageStatus), status))
            {
                throw new AbpValidationException();
            }

            Status = status;

            return this;
        }

    }
}
