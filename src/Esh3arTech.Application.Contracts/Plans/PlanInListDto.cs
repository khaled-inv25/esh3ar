using System;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.Plans
{
    public class PlanInListDto : EntityDto<Guid>
    {
        public string DisplayName { get; set; }

        public string? ExpiringPlan { get; set; }

        public decimal? DailyPrice { get; set; }

        public decimal? WeeklyPrice { get; set; }

        public decimal? MonthlayPrice { get; set; }

        public decimal? AnnualPrice { get; set; }

        public int? TrialDayCount { get; set; }

        public int? WaitingDayAfterExpire { get; set; }
    }
}
