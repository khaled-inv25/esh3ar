using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Validation;

namespace Esh3arTech.Messages
{
    [Table(Esh3arTechConsts.TblMessage)]
    public class Message : FullAuditedAggregateRoot<Guid>
    {
        public string RecipientPhoneNumber { get; private set; }

        public string Subject { get; private set; }

        public string? MessageContent { get; private set; }

        public MessageStatus Status { get; private set; }

        public MessageType Type { get; private set; }

        public Guid? ConversationId { get; private set; }

        public string? SenderPhoneNumber { get; private set; }

        public int RetryCount { get; private set; }

        public DateTime? DeliveredAt { get; private set; }

        public DateTime? ReadAt { get; private set; }

        public string? FailureReason { get; private set; }

        public DateTime? LastRetryAt { get; private set; }

        public DateTime? NextRetryAt { get; private set; }

        public Priority Priority { get; private set; }

        public ICollection<MessageAttachment> Attachments { get; private set; }

        [NotMapped]
        public short MaxAllowedAttchments { get; private set; }

        public Message(
            Guid id,
            string recipientPhoneNumber,
            MessageType type)
            : base(id)
        {
            SetRecipientPhoneNumber(recipientPhoneNumber);
            SetMessageType(type);
            SetPriority(Priority.Normal);
            Attachments = new Collection<MessageAttachment>();
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

        public Message SetMessageContentOrNull(string? content = null)
        {
            MessageContent = content;

            return this;
        }

        public Message SetMessageStatusType(MessageStatus status)
        {
            if (!Enum.IsDefined(typeof(MessageStatus), status))
            {
                throw new AbpValidationException();
            }

            if (status.Equals(MessageStatus.Delivered))
            {
                MarkAsDelivered();
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

        public Message AddAttachment(Guid attachmentId, string fileName, string type, string accessUrl, DateTime? urlExpiresAt = null)
        {
            IncrementAttachmentCount();
            Attachments.Add(new MessageAttachment(attachmentId, Id, fileName, type, accessUrl, urlExpiresAt));
            return this;
        }

        public Message SetFailureReason(string reason)
        {
            FailureReason = Check.NotNullOrEmpty(reason, nameof(reason));
            return this;
        }

        public Message IncrementRetryCount()
        {
            RetryCount++;
            LastRetryAt = DateTime.UtcNow;

            return this;
        }

        public Message ScheduleNextRetry(TimeSpan delay)
        {
            NextRetryAt = DateTime.UtcNow.Add(delay);

            return this;
        }

        public Message MarkAsRetrying()
        {
            if (Status != MessageStatus.Failed && Status != MessageStatus.Queued)
            {
                throw new BusinessException("Only failed or queued messages can be marked as retrying.");
            }

            Status = MessageStatus.Queued;

            return this;
        }

        private void MarkAsDelivered()
        {
            DeliveredAt = (Status.Equals(MessageStatus.Delivered)) ? DateTime.Now : null;
        }

        private void IncrementAttachmentCount()
        {
            if (Attachments != null && Attachments.Count >= AttachmenstConsts.MaxAllowedAttchments)
            {
                throw new BusinessException("Maximum number of attachments reached.");
            }

            MaxAllowedAttchments++;
        }

    }
}
