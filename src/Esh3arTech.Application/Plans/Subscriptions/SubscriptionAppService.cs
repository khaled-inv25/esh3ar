using Esh3arTech.Plans;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace Esh3arTech.Plans.Subscriptions
{
    public class SubscriptionAppService : Esh3arTechAppService, ISubscriptionAppService
    {

        private readonly IRepository<Subscription, Guid> _subscriptionRepository;
        private readonly IIdentityUserRepository _identityUserRepository;
        private readonly IUserPlanRepository _userPlanRepository;
        private readonly SubscriptionManagere _subscriptionManagere;

        public SubscriptionAppService(
            IRepository<Subscription, Guid> subscriptionRepository,
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

            var createdSubscription = _subscriptionManagere.AssignInitialPlan(user, plan, input.BillingInterval, input.Price);

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
        }

        public async Task<PagedResultDto<SubscriptionInListDto>> GetAllSubscriptionsAsync()
        {

            return null;
        }
    }
}
