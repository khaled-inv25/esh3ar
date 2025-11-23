using Esh3arTech.UserPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Esh3arTech.EntityFrameworkCore.UserPlans
{
    public class UserPlanRepository : EfCoreRepository<Esh3arTechDbContext, UserPlan, Guid>, IUserPlanRepository
    {
        public UserPlanRepository(IDbContextProvider<Esh3arTechDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public override async Task<IQueryable<UserPlan>> WithDetailsAsync()
        {
            return await GetQueryableAsync();
        }
    }
}
