using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace Esh3arTech.UserPlans
{
    [Table(Esh3arTechConsts.TblUserPlan)]
    public class UserPlan : FullAuditedEntity<Guid>
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public Guid? ExpiringPlanId { get; set; }

        public decimal? DailyPrice { get; set; }

        public decimal? WeeklyPrice { get; set; }

        public decimal? MonthlayPrice { get; set; }

        public decimal? AnnualPrice { get; set; }

        public int? TrialDayCount { get; set; }

        public int? WaitingDayAfterExpire { get; set; }

    }
}
