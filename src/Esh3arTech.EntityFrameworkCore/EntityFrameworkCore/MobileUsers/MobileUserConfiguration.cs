using Esh3arTech.MobileUsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esh3arTech.EntityFrameworkCore.MobileUsers
{
    public class MobileUserConfiguration : IEntityTypeConfiguration<MobileUser>
    {
        public void Configure(EntityTypeBuilder<MobileUser> builder)
        {
            builder.Property(m => m.MobileNumber).IsRequired().HasMaxLength(MobileUserConsts.MaxMobileNumberLength);
            builder.Property(m => m.Status).IsRequired();
            builder.Property(m => m.IsStatic).IsRequired();

            // Configure one-to-many relationship: MobileUser -> RegistrationRequests
            builder
                .HasMany(m => m.Requests)
                .WithOne(r => r.MobileUser)
                .HasForeignKey(r => r.MobileUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
