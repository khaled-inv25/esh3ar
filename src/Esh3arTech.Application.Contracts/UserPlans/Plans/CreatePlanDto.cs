using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Esh3arTech.UserPlans.Plans
{
    public record CreatePlanDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [AllowNull]
        public Guid? ExpiringPlanId { get; set; }

        [AllowNull]
        [Range(1, double.MaxValue)]
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
