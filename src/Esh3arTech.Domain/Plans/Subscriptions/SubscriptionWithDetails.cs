using System;

namespace Esh3arTech.Plans.Subscriptions
{
    public class SubscriptionWithDetails
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public string Plan { get; set; }

        public decimal Price { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime NextBill { get; set; }

    }
}
