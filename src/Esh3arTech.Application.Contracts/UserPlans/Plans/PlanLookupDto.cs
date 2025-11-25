using System;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.UserPlans.Plans
{
    // Specifically for filling the drowpdown.
    public class PlanLookupDto : EntityDto<Guid>
    {
        public string DisplayName { get; set; }
    }
}
