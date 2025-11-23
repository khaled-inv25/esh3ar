using Volo.Abp.Application.Dtos;

namespace Esh3arTech.UserPlans.Plans
{
    public class PlanListFilter : PagedResultRequestDto
    {
        public PlanListFilter()
        {
            MaxMaxResultCount = 25;
        }
    }
}
