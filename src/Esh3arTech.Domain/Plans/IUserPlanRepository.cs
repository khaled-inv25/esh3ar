using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Esh3arTech.Plans
{
    public interface IUserPlanRepository : IRepository<UserPlan, Guid>
    {
        Task<int> GetLinkedUsersCountToPlan(Guid planId);

        Task<int> GetLinkedUsersCountWithPlan();

        Task<bool> IsThereLinkedUsersAsync(Guid planId);

        Task MoveUsersToPlan(Guid planId);

        Task AssginPlanToUser(Guid planId, Guid userId);

        Task<bool> IsPlanFreeById(Guid planId);
    }
}
