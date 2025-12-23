using Esh3arTech.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esh3arTech.EntityFrameworkCore.Messages
{
    public class AttachmentConfiguration : IEntityTypeConfiguration<MessageAttachment>
    {
        public void Configure(EntityTypeBuilder<MessageAttachment> builder)
        {
            builder.Property(ma => ma.MessageId).IsRequired();

            builder.Property(ma => ma.FileName).IsRequired().HasMaxLength(AttachmenstConsts.MaxFileNameLength);
            
            builder.Property(ma => ma.AccessUrl).IsRequired().HasMaxLength(AttachmenstConsts.MaxAccessUrlLength);

            builder.Property(ma => ma.UrlExpiresAt).IsRequired(false);
        }
    }
}
