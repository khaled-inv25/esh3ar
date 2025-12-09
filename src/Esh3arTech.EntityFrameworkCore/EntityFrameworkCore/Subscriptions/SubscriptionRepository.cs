using Esh3arTech.Plans;
using Esh3arTech.Plans.Subscriptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;

namespace Esh3arTech.EntityFrameworkCore.Subscriptions
{
    public class SubscriptionRepository : EfCoreRepository<Esh3arTechDbContext, Subscription, Guid>, ISubscriptionRepository
    {

        public SubscriptionRepository(
            IDbContextProvider<Esh3arTechDbContext> dbContextProvider) 
            : base(dbContextProvider)
        {
        }

        public async Task<List<SubscriptionWithDetails>> GetAllSubsccriptionsWithDetailsAsync()
        {
            var dbContext = await GetDbContextAsync();

            var usersDbSet = dbContext.Set<IdentityUser>();
            var planDbSet = dbContext.Set<UserPlan>();

            var query = from subscription in dbContext.Subscriptions
                        join users in usersDbSet on subscription.UserId equals users.Id
                        join plans in planDbSet on subscription.PlanId equals plans.Id
                        select new SubscriptionWithDetails
                        {
                            Id = subscription.Id,
                            UserName = users.UserName,
                            Plan = plans.Name,
                            Price = subscription.Price,
                            StartDate = subscription.StartDate,
                            EndDate = subscription.EndDate,
                            NextBill = subscription.NextBill
                        };

            return await AsyncExecuter.ToListAsync(query);
        }

        public override async Task<IQueryable<Subscription>> WithDetailsAsync()
        {
            return (await GetQueryableAsync())
                .Include(s => s.RenewalHistories);
        }
    }
}
