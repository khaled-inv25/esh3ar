using Esh3arTech.UserPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esh3arTech.Configuration
{
    public class UserPlanConfiguration : IEntityTypeConfiguration<UserPlan>
    {
        public void Configure(EntityTypeBuilder<UserPlan> builder)
        {
            builder.Property(up => up.Name).IsRequired();
            builder.Property(up => up.DisplayName).IsRequired();
            builder.Property(up => up.ExpiringPlanId).IsRequired(false);

            builder.Property(up => up.DailyPrice)
                .HasColumnType("decimal(18,4)")
                .IsRequired(false);

            builder.Property(up => up.WeeklyPrice)
                .HasColumnType("decimal(18,4)")
                .IsRequired(false);

            builder.Property(up => up.MonthlayPrice)
                .HasColumnType("decimal(18,4)")
                .IsRequired(false);

            builder.Property(up => up.AnnualPrice)
                .HasColumnType("decimal(18,4)")
                .IsRequired(false);

            builder.Property(up => up.TrialDayCount)
                .HasColumnType("decimal(18,4)")
                .IsRequired(false);

            builder.Property(up => up.WaitingDayAfterExpire)
                .HasColumnType("decimal(18,4)")
                .IsRequired(false);
        }
    }
}
