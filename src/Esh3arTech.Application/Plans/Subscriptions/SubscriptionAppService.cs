using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;
using Volo.Abp.ObjectMapping;

namespace Esh3arTech.Plans.Subscriptions
{
    public class SubscriptionAppService : Esh3arTechAppService, ISubscriptionAppService
    {

        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IIdentityUserRepository _identityUserRepository;
        private readonly IUserPlanRepository _userPlanRepository;
        private readonly SubscriptionManagere _subscriptionManagere;

        public SubscriptionAppService(
            ISubscriptionRepository subscriptionRepository,
            IIdentityUserRepository identityUserRepository,
            IUserPlanRepository userPlanRepository,
            SubscriptionManagere subscriptionManagere)
        {
            _subscriptionRepository = subscriptionRepository;
            _identityUserRepository = identityUserRepository;
            _userPlanRepository = userPlanRepository;
            _subscriptionManagere = subscriptionManagere;
        }

        public async Task AssignSubscriptionToUser(AssignSubscriptionToUserDto input)
        {
            var user = await _identityUserRepository.GetAsync(input.UserId);
            var plan = await _userPlanRepository.GetAsync(input.PlanId);

            var createdSubscription = _subscriptionManagere.AssignPlan(user, plan, input.BillingInterval, input.Price);

            if (input.IsAutoRenew)
            {
                createdSubscription.AutoRenew();
            }

            if (input.IsActive)
            {
                createdSubscription.Active();
                createdSubscription.SetLastPaymentStatus();
            }

            await _subscriptionRepository.InsertAsync(createdSubscription);
            await _userPlanRepository.AssginPlanToUser(plan.Id, user.Id);
        }

        public async Task<PagedResultDto<SubscriptionInListDto>> GetAllSubscriptionsAsync()
        {
            var subscriptionsWithDetails = await _subscriptionRepository.GetAllSubsccriptionsWithDetailsAsync();

            return new PagedResultDto<SubscriptionInListDto>(subscriptionsWithDetails.Count, 
                ObjectMapper.Map<List<SubscriptionWithDetails>, List<SubscriptionInListDto>>(subscriptionsWithDetails));
        }

        public async Task<SubscriptionDto> GetSubscriptionByIdAsync(Guid subscriptionId)
        {
            return ObjectMapper.Map<Subscription, SubscriptionDto>(await _subscriptionRepository.GetAsync(subscriptionId));
        }

        public async Task RenewSubscription(RenewSubscriptionDto input)
        {
            var subscription = await _subscriptionRepository.GetAsync(input.SubscriptionId);

            // Calculate the new period based on the billing interval.
            // Or if you have a custom price for the new renew.

            // extend the subscription period, by custom period or by billing interval.

            await _subscriptionManagere.RenewSubscription(subscription);


            if (subscription.Price != input.Price)
            {
                subscription.SetPrice(input.Price);
            }

        }
    }
}
