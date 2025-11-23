using System.Collections.Generic;

namespace Esh3arTech.UserPlans.Plans
{
    public record PlanDto
    {
        public string Name { get; set; }

        public List<PlanFeatureDto> Features { get; set; }
    }
}
