using System;

namespace Esh3arTech.Plans
{
    public record AssignPlanToUserDto
    {
        public Guid PlanId { get; set; }

        public Guid UserId { get; set; }
    }
}
