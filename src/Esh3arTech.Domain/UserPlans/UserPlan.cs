using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Esh3arTech.UserPlans
{
    [Table(Esh3arTechConsts.TblUserPlan)]
    public class UserPlan : FullAuditedEntity<Guid>
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public Guid? ExpiringPlanId { get; private set; }

        public decimal? DailyPrice { get; private set; }

        public decimal? WeeklyPrice { get; private set; }

        public decimal? MonthlayPrice { get; private set; }

        public decimal? AnnualPrice { get; private set; }

        public int? TrialDayCount { get; set; }

        public int? WaitingDayAfterExpire { get; set; }

        [NotMapped]
        public bool IsFree => !DailyPrice.HasValue && !WeeklyPrice.HasValue && !MonthlayPrice.HasValue && !AnnualPrice.HasValue;

        public UserPlan(Guid id, string name, string displayName) 
            : base(id)
        {
            Name = Check.NotNullOrEmpty(name, nameof(name));
            DisplayName = Check.NotNullOrEmpty(displayName, nameof(displayName));
        }

        internal UserPlan SetExpiringPlanId(Guid? expiringPlanId)
        {
            if (!ExpiringPlanId.HasValue)
            {
                ExpiringPlanId = null;
            }

            ExpiringPlanId = expiringPlanId;
            return this;
        }

        public bool HasTrialDays()
        {
            if (!IsFree)
            {
                return false;
            }

            return TrialDayCount.HasValue && TrialDayCount.Value > 0;
        }

        public UserPlan SetPriceInfo(decimal? dailyPrice, decimal? weeklyPrice, decimal? monthlayPrice, decimal? annualPrice)
        {
            var hasPrice = dailyPrice.HasValue && weeklyPrice.HasValue && monthlayPrice.HasValue && annualPrice.HasValue;
            if (!hasPrice)
            {
                DailyPrice = null;
                WeeklyPrice = null;
                MonthlayPrice = null;
                AnnualPrice = null;
            }
            else
            {
                DailyPrice = Check.Range(dailyPrice.Value, parameterName: nameof(dailyPrice), minimumValue: 0.0000001m, maximumValue: decimal.MaxValue);
                WeeklyPrice = Check.Range(weeklyPrice.Value, parameterName: nameof(weeklyPrice), minimumValue: 0.0000001m, maximumValue: decimal.MaxValue);
                MonthlayPrice = Check.Range(monthlayPrice.Value, parameterName: nameof(monthlayPrice), minimumValue: 0.0000001m, maximumValue: decimal.MaxValue);
                AnnualPrice = Check.Range(annualPrice.Value, parameterName: nameof(annualPrice), minimumValue: 0.0000001m, maximumValue: decimal.MaxValue);
            }

            return this;
        }

        internal UserPlan SetTrialDayCount(int? trialDay)
        {
            if (!trialDay.HasValue || IsFree)
            {
                TrialDayCount = null;
                return this;
            }

            TrialDayCount = Check.Range(trialDay.Value, parameterName: nameof(trialDay), minimumValue: 0, maximumValue: 15);
            return this;
        }

        internal UserPlan SetWaitingDayAfterExpire(int? waitingDayAfterExpire)
        {
            if (!waitingDayAfterExpire.HasValue || IsFree)
            {
                WaitingDayAfterExpire = null;
                return this;
            }

            WaitingDayAfterExpire = Check.Range(waitingDayAfterExpire.Value, parameterName: nameof(waitingDayAfterExpire), minimumValue: 0, maximumValue: 15);

            return this;
        }
    }
}
