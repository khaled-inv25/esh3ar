using System;

namespace Esh3arTech.UserPlans.Plans
{
    public record AssignPlanToUserDto
    {
        public Guid PlanId { get; set; }

        public Guid UserId { get; set; }
    }
}
