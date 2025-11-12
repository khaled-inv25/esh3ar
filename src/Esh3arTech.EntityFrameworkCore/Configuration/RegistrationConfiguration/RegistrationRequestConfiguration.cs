using Esh3arTech.Registretions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esh3arTech.Configuration.RegistrationConfiguration
{
    public class RegistrationRequestConfiguration : IEntityTypeConfiguration<RegistretionRequest>
    {
        public void Configure(EntityTypeBuilder<RegistretionRequest> builder)
        {
            builder.Property(r => r.OS).IsRequired();
            builder.Property(r => r.Verified).IsRequired();
            builder.Property(r => r.VerifiedTime).IsRequired(false);

            builder.Property(r => r.MobileUserId).IsRequired();
        }
    }
}
