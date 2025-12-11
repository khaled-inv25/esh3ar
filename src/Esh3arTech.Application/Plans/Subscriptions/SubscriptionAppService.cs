using System;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;

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
            var subscription = await _subscriptionRepository.GetAsync(input.SubscriptionId, true);

            if (subscription.Price != input.Price)
            {
                subscription.SetPrice(input.Price);
            }

            await _subscriptionManagere.RenewSubscription(subscription, input.Price);

            await _subscriptionRepository.UpdateAsync(subscription);
        }

        public async Task<PagedResultDto<SubscriptionHistoryInListDto>> GetSubscriptionHistoryByIdAsync(SubscriptionFilterDto input)
        {
            IReadOnlyList<SubscriptionRenewalHistory> history;
            if (Guid.TryParse(input.SubscriptionId, out Guid subscriptionId))
            {
                var subscription = await _subscriptionRepository.GetAsync(subscriptionId, true);
                history = subscription.GetRenewalHistories();
            }
            else
            {
                throw new UserFriendlyException("Invalid GUID string");
            }

            return new PagedResultDto<SubscriptionHistoryInListDto>(history.Count, 
                ObjectMapper.Map<IReadOnlyList<SubscriptionRenewalHistory>, IReadOnlyList<SubscriptionHistoryInListDto>>(history));
        }
    }
}
