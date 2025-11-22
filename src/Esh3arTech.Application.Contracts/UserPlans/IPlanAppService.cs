using System.Threading.Tasks;

namespace Esh3arTech.UserPlans
{
    public interface IPlanAppService
    {
        Task<PlanDto> GetCurrentUserPlanAsync();

        Task<PlanDto> GetPlanByIdAsync(string planId);

        Task<PlanInListDto> GetAllPlansAsync();

        Task AssginPlanToUserAsync(AssignPlanToUserDto input);

        Task CreatePlanAsync(CreatePlanDto input);
    }
}
