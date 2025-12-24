using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Esh3arTech.Messages
{
    [Table(Esh3arTechConsts.TblMessageAttachment)]
    public class MessageAttachment : Entity<Guid>
    {
        public Guid MessageId { get; private set; }

        public string FileName { get; private set; }

        public string Type { get; private set; }

        public string AccessUrl { get; private set; }

        public DateTime? UrlExpiresAt { get; private set; }

        public MessageAttachment(
            Guid Id,
            Guid messageId, 
            string fileName,
            string type,
            string accessUrl, 
            DateTime? urlExpiresAt = null)
            : base(Id)
        {
            MessageId = messageId;
            FileName = fileName;
            Type = type;
            SetAccessUrl(accessUrl);
            UrlExpiresAt = urlExpiresAt;
        }

        private MessageAttachment SetAccessUrl(string accessUrl)
        {
            AccessUrl = Check.NotNullOrEmpty($"{accessUrl}/{FileName}", nameof(accessUrl));
            return this;
        }
    }
}
