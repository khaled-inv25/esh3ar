using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace Esh3arTech.Messages.Attachments
{
    [Table(Esh3arTechConsts.TblMessageAttachment)]
    public class MessageAttachment : FullAuditedEntity<Guid>
    {
        public Guid? MessageId { get; private set; }

        public string FileName { get; private set; }

        public ContentType Type { get; private set; }

        public long FileSize { get; private set; }

        public string BlobName { get; private set; }

        public string AccessUrl { get; private set; }

        public DateTime? UrlExpiresAt { get; private set; }

        [NotMapped]
        public short MaxAllowedAttchments { get; private set; }
    }
}
