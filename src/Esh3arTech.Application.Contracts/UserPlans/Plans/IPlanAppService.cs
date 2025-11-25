using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.UserPlans.Plans
{
    public interface IPlanAppService
    {
        //Task<PlanDto> GetCurrentUserPlanAsync(); // Move to (IUserPlanAppService)
        Task<PagedResultDto<PlanInListDto>> GetAllPlansAsync(PlanListFilter input);

        Task<PlanDto> GetPlanByIdAsync(Guid planId);

        Task AssginPlanToUserAsync(AssignPlanToUserDto input);

        Task CreatePlanAsync(CreatePlanDto input);

        Task<List<PlanFeatureDto>> GetDefaultFeaturesAsync();

        Task<List<PlanLookupDto>> GetPlanLookupAsync();
    }
}
