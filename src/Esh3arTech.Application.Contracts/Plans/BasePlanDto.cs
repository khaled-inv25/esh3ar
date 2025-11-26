using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System;

namespace Esh3arTech.Plans
{
    public class BasePlanDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [AllowNull]
        public Guid? ExpiringPlanId { get; set; }

        [AllowNull]
        [Range(0.00001, double.MaxValue)]
        public decimal? DailyPrice { get; set; }

        [AllowNull]
        public decimal? WeeklyPrice { get; set; }

        [AllowNull]
        public decimal? MonthlayPrice { get; set; }

        [AllowNull]
        public decimal? AnnualPrice { get; set; }

        [AllowNull]
        public int? TrialDayCount { get; set; }

        [AllowNull]
        public int? WaitingDayAfterExpire { get; set; }

        public List<PlanFeatureDto> Features { get; set; } = new();
    }
}
