using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace Esh3arTech.Editions
{
    [Table(Esh3arTechConsts.TblEditions)]
    public class Edition : FullAuditedEntity<Guid>
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public Guid? ExpiringEditionId { get; set; }

        public decimal? DailyPrice { get; set; }

        public decimal? WeeklyPrice { get; set; }

        public decimal? MonthlayPrice { get; set; }

        public decimal? AnnualPrice { get; set; }

        public int? TrialDayCount { get; set; }

        public int? WaitingDayAfterExpire { get; set; }

    }
}
