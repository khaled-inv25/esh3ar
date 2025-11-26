using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.Plans
{
    public interface IPlanAppService
    {
        Task<PlanDto> GetUserPlanInfoAsync(Guid userId);

        Task<PagedResultDto<PlanInListDto>> GetAllPlansAsync(PlanListFilter input);

        Task<PlanDto> GetPlanByIdAsync(Guid planId);

        Task CreatePlanAsync(CreatePlanDto input);

        Task<List<PlanFeatureDto>> GetDefaultFeaturesAsync();

        Task<List<PlanLookupDto>> GetPlanLookupAsync();

        Task UpdateAsync(Guid Id, UpdatePlanDto input);

        Task AssignAfallback(ExpireToPlanDto input);

        Task<int> GetAssignedUsersCountForPlanAsync(Guid planId);

        Task DeleteAsync(Guid planId);

        Task<int> GetLinkedUsersCountToPlan(Guid planId);

        Task<int> GetLinkedUsersCountWithPlan();

        Task MoveUsersToPlan(Guid planId);
    }
}
