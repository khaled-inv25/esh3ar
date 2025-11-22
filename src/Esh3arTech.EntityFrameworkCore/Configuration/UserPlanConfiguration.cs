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
            builder.Property(up => up.DailyPrice).IsRequired(false);
            builder.Property(up => up.WeeklyPrice).IsRequired(false);
            builder.Property(up => up.MonthlayPrice).IsRequired(false);
            builder.Property(up => up.AnnualPrice).IsRequired(false);
            builder.Property(up => up.TrialDayCount).IsRequired(false);
            builder.Property(up => up.WaitingDayAfterExpire).IsRequired(false);
        }
    }
}
