using Esh3arTech.Plans;
using Esh3arTech.Plans.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.Identity;

namespace Esh3arTech.EntityFrameworkCore.Subscriptions
{
    // The configuration of the subscription entity.
    public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(s => s.UserId).IsRequired().OnDelete(DeleteBehavior.NoAction);
            
            builder.HasOne<UserPlan>()
                .WithMany()
                .HasForeignKey(s => s.PlanId).IsRequired().OnDelete(DeleteBehavior.NoAction);

            builder.Property(s => s.Price)
                .HasColumnType("decimal(18,4)")
                .IsRequired();

            builder.Property(s => s.BillingInterval)
                .IsRequired();

            builder.Property(s => s.StartDate).IsRequired();
            builder.Property(s => s.EndDate).IsRequired();
            builder.Property(s => s.NextBill).IsRequired();

            builder.Property(s => s.IsAutoRenew).IsRequired();
            builder.Property(s => s.IsActive).IsRequired();

            builder.Property(s => s.LastPaymentStatus).IsRequired();
            builder.Property(s => s.Status).IsRequired();


        }
    }

    // The configuration of the subscription renewal history entity.
    public class SubscriptionRenewalHistoryConfiguration : IEntityTypeConfiguration<SubscriptionRenewalHistory>
    {
        public void Configure(EntityTypeBuilder<SubscriptionRenewalHistory> builder)
        {
            builder.HasOne<Subscription>()
                .WithMany(s => s.RenewalHistories)
                .HasForeignKey(srh => srh.SubscriptionId).IsRequired().OnDelete(DeleteBehavior.NoAction);

            builder.Property(srh => srh.RenewalDate).IsRequired();
            builder.Property(srh => srh.PeriodStartDate).IsRequired();
            builder.Property(srh => srh.PeriodEndDate).IsRequired();
            builder.Property(srh => srh.Amount)
                .HasColumnType("decimal(18,4)")
                .IsRequired();
            builder.Property(srh => srh.BillingInterval).IsRequired();
            builder.Property(srh => srh.IsManual).IsRequired();

        }
    }
}
