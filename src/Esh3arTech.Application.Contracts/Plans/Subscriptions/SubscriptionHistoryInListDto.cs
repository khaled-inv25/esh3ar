using System;

namespace Esh3arTech.Plans.Subscriptions
{
    public class SubscriptionHistoryInListDto
    {
        public Guid SubscriptionId { get; set; }

        public DateTime RenewalDate { get; set; }

        public DateTime PeriodStartDate { get; set; }

        public DateTime PeriodEndDate { get; set; }

        public decimal Amount { get; set; }

        public BillingInterval BillingInterval { get; set; }

        public InitialAssignment Type { get; set; }

        public bool IsManual { get; set; }
    }
}
