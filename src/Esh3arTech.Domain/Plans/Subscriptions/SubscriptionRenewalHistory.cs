using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace Esh3arTech.Plans.Subscriptions
{
    [Table(Esh3arTechConsts.TblSubscriptionRenewalHistory)]
    public class SubscriptionRenewalHistory : FullAuditedEntity<Guid>
    {
        public Guid SubscriptionId { get; set; }

        public DateTime RenewalDate { get; set; }

        public DateTime PeriodStartDate { get; set; }

        public DateTime PeriodEndDate { get; set; }

        public decimal Amount { get; set; }

        public BillingInterval BillingInterval { get; set; }

        public InitialAssignment Type { get; set; }

        public bool IsManual { get; set; }

        public SubscriptionRenewalHistory(
            Guid subscriptionId, 
            DateTime renewalDate, 
            DateTime periodStartDate, 
            DateTime periodEndDate, 
            decimal amount, 
            BillingInterval billingInterval, 
            bool isManual,
            InitialAssignment type)
        {
            SubscriptionId = subscriptionId;
            RenewalDate = renewalDate;
            PeriodStartDate = periodStartDate;
            PeriodEndDate = periodEndDate;
            Amount = amount;
            BillingInterval = billingInterval;
            IsManual = isManual;
            Type = type;
        }

    }
}
