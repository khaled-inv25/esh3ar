using Esh3arTech.Messages;
using Esh3arTech.MobileUsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.Identity;

namespace Esh3arTech.EntityFrameworkCore.Messages
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(m => m.CreatorId).IsRequired().OnDelete(DeleteBehavior.NoAction);

            builder.Property(m => m.RecipientPhoneNumber)
                .HasMaxLength(MessageConts.MaxPhoneNumberLength)
                .IsRequired();
            
            builder.Property(m => m.Subject)
                .HasMaxLength(MessageConts.MaxSubjectLength)
                .IsRequired(false);
            
            builder.Property(m => m.Status)
                .IsRequired();

            builder.Property(m => m.Type)
                .IsRequired();

            builder.Property(m => m.ConversationId)
                .IsRequired(false);

            builder.Property(m => m.SenderPhoneNumber)
                .HasMaxLength(MessageConts.MaxPhoneNumberLength)
                .IsRequired(false);

            builder.Property(m => m.RetryCount)
                .IsRequired();

            builder.Property(m => m.DeliveredAt)
                .IsRequired(false);

            builder.Property(m => m.ReadAt)
                .IsRequired(false);
        }
    }
}
