using Esh3arTech.Plans;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;

namespace Esh3arTech.EntityFrameworkCore.Plans
{
    public class UserPlanRepository : EfCoreRepository<Esh3arTechDbContext, UserPlan, Guid>, IUserPlanRepository
    {
        public UserPlanRepository(IDbContextProvider<Esh3arTechDbContext> dbContextProvider) 
            : base(dbContextProvider)
        {
        }

        // To get the count of users linked to a specific plan.
        public async Task<int> GetLinkedUsersCountToPlan(Guid planId)
        {
            var dbContext = await GetDbContextAsync();
            var dbSetIdentityUser = dbContext.Set<IdentityUser>();

            return dbSetIdentityUser.Count(u => EF.Property<string>(u, "PlanId").Equals(planId.ToString()));
        }

        // To get the count of users linked to any plan.
        public async Task<int> GetLinkedUsersCountWithPlan()
        {
            var dbContext = await GetDbContextAsync();
            var dbSetIdentityUser = dbContext.Set<IdentityUser>();

            var count = dbSetIdentityUser.Where(u => EF.Property<string>(u, "PlanId") != null).Count();

            return count;
        }

        // To get whether there are users linked to a specific plan using PlanId witch is not a property of IdentityUser.
        // It was added dynamically to IdentityUser in the Esh3arTechEfCoreEntityExtensionMappings class.
        public async Task<bool> IsThereLinkedUsersAsync(Guid planId)
        {
            var dbContext = await GetDbContextAsync();
            var dbSetIdentityUser = dbContext.Set<IdentityUser>();

            var isUserLinkedToPlan = dbSetIdentityUser.Any(u => EF.Property<string>(u, "PlanId").Equals(planId.ToString()));

            return isUserLinkedToPlan;
        }

        public async Task MoveUsersToPlan(Guid planId)
        {
            var dbContext = await GetDbContextAsync();
            var dbSetIdentityUser = dbContext.Set<IdentityUser>();

            var usersToUpdate = dbSetIdentityUser.ToList();

            foreach (var user in usersToUpdate)
            {
                
                if (user.UserName.Equals("admin") || user.GetProperty<string>("PlanId") == null)
                {
                    continue;
                }
                user.SetProperty("PlanId", planId.ToString());
                dbSetIdentityUser.Update(user);
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task AssginPlanToUser(Guid planId, Guid userId)
        {
            var dbContext = await GetDbContextAsync();
            var dbSetIdentityUser = dbContext.Set<IdentityUser>();

            var user = await dbSetIdentityUser.FirstAsync(u => u.Id == userId);
            user.SetProperty("PlanId", planId.ToString());
            dbSetIdentityUser.Update(user);

            await dbContext.SaveChangesAsync();
        }

        public async Task<bool> IsPlanFreeById(Guid planId)
        {
            var planQuery = from up in await GetQueryableAsync()
                            where up.Id == planId
                            select new
                            {
                                IsFree = (up.DailyPrice ?? 0) == 0 &&
                                         (up.WeeklyPrice ?? 0) == 0 &&
                                         (up.MonthlayPrice ?? 0) == 0 &&
                                         (up.AnnualPrice ?? 0) == 0
                            };

            var queryResult = await AsyncExecuter.FirstOrDefaultAsync(planQuery);

            return queryResult?.IsFree ?? false;
        }

        //public override async Task<IQueryable<UserPlan>> WithDetailsAsync()
        //{
        //    return await GetQueryableAsync();
        //}

    }
}
