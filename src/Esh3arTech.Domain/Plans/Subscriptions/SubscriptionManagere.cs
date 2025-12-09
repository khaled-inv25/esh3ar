using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;

namespace Esh3arTech.Plans.Subscriptions
{
    public class SubscriptionManagere : DomainService
    {
        private readonly IUserPlanRepository _userPlanRepository;

        public SubscriptionManagere(IUserPlanRepository userPlanRepository)
        {
            _userPlanRepository = userPlanRepository;
        }

        public Subscription AssignPlan(IdentityUser user, UserPlan plan, BillingInterval billingInterval, decimal price)
        {
            var subscription = new Subscription(
                GuidGenerator.Create(),
                user.Id,
                plan.Id,
                billingInterval
            );

            subscription.SetInitialPeriod();

            if (plan.IsFree)
            {
                subscription.SetPrice(0);
            }
            else
            {
                // To check if the price is provided from outside.
                if (price.Equals(0)) 
                {
                    subscription.SetPrice(CalcCostBaseOnPlanPrice(billingInterval, plan));
                }
                else
                {
                    subscription.SetPrice(price);
                }
            }
            subscription.SetNextBilling();
            subscription.AddHistory(price);

            return subscription;
        }

        public async Task<Subscription> RenewSubscription(Subscription subscription, decimal amount)
        {
            if (await _userPlanRepository.IsPlanFreeById(subscription.PlanId))
            {
                throw new BusinessException("Cannot renew a free plan subscription.");
            }

            if (!subscription.IsActive)
            {
                throw new UserFriendlyException("Cannot renew an inactive subscription. please active this subscription or contact the admin");
            }

            subscription.RenewManually(amount, InitialAssignment.Renewal);

            return subscription;
        }

        private decimal CalcCostBaseOnPlanPrice(BillingInterval billingInterval, UserPlan plan)
        {
            return billingInterval switch
            {
                BillingInterval.Daily => plan.DailyPrice!.Value,
                BillingInterval.Weekly => plan.WeeklyPrice!.Value,
                BillingInterval.Monthly => plan.MonthlayPrice!.Value,
                BillingInterval.Annually => plan.AnnualPrice!.Value,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }
}
