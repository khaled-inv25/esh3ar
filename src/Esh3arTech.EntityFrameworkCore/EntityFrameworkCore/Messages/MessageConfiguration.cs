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
                .HasMaxLength(MessageConts.MaxRecipientPhoneNumberLength)
                .IsRequired();
            
            builder.Property(m => m.Subject)
                .HasMaxLength(MessageConts.MaxSubjectLength)
                .IsRequired();
            
            builder.Property(m => m.ContentType)
                .IsRequired();
            
            builder.Property(m => m.Status)
                .IsRequired();
        }
    }
}
