using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Esh3arTech.Plans.Subscriptions
{
    public interface ISubscriptionRepository : IRepository<Subscription, Guid>
    {
        Task<List<SubscriptionWithDetails>> GetAllSubsccriptionsWithDetailsAsync();

    }
}
