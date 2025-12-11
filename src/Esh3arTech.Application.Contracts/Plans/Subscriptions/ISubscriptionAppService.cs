using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.Plans.Subscriptions
{
    public interface ISubscriptionAppService
    {
        Task AssignSubscriptionToUser(AssignSubscriptionToUserDto input);

        Task<PagedResultDto<SubscriptionInListDto>> GetAllSubscriptionsAsync();

        Task RenewSubscription(RenewSubscriptionDto input);

        Task<SubscriptionDto> GetSubscriptionByIdAsync(Guid subscriptionId);

        Task<PagedResultDto<SubscriptionHistoryInListDto>> GetSubscriptionHistoryByIdAsync(SubscriptionFilterDto input);
    }
}
