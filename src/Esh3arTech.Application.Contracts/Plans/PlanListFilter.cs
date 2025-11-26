using Volo.Abp.Application.Dtos;

namespace Esh3arTech.Plans
{
    public class PlanListFilter : PagedResultRequestDto
    {
        public PlanListFilter()
        {
            MaxMaxResultCount = 25;
        }
    }
}
