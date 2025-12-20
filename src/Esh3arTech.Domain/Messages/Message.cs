using System;
using System.ComponentModel.DataAnnotations.Schema;
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

        public string MessageContent { get; private set; }

        public MessageStatus Status { get; private set; }

        public MessageType Type { get; private set; }

        public Guid? ConversationId { get; private set; }

        public string? SenderPhoneNumber { get; private set; }

        public int RetryCount { get; private set; }

        public DateTime? DeliveredAt { get; private set; }

        public DateTime? ReadAt { get; private set; }

        public string? FailureReason { get; private set; }

        public Priority Priority { get; private set; }
        
        // Updated Ctor 
        public Message(
            Guid id,
            string recipientPhoneNumber,
            MessageType type ) 
            : base(id)
        {
            SetRecipientPhoneNumber(recipientPhoneNumber);
            SetMessageType(type);
            SetPriority(Priority.Normal);
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

        public Message SetMessageType(MessageType type)
        {
            if (!Enum.IsDefined(typeof(MessageType), type))
            {
                throw new AbpValidationException();
            }

            Type = type;
            return this;
        }

        public Message SetPriority(Priority priority)
        {
            if (!Enum.IsDefined(typeof(Priority), priority))
            {
                throw new AbpValidationException();
            }

            Priority = priority;
            return this;
        }

        /*
         * * Methods needed:
         * 
         * 
         * SetMessageType ✅
         * MarkAsDelivered
         * IncrementRetryCount
         * SetConversation
         * MarkAsRead
         * MarkAsFailed --> we should have the reson for failure.
         */

    }
}
