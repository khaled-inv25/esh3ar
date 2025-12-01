using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;

namespace Esh3arTech.Plans.Subscriptions
{
    public class SubscriptionManagere : DomainService
    {
        public Subscription AssignInitialPlan(IdentityUser user, UserPlan plan, BillingInterval billingInterval, decimal price)
        {
            var subscription = new Subscription(
                GuidGenerator.Create(),
                user.Id,
                plan.Id,
                billingInterval
            );
            subscription.SetInitialPeriod();
            subscription.SetPrice(price);
            subscription.SetNextBilling();

            return subscription;
        }
    }
}
