using System;

namespace Esh3arTech.Plans.Subscriptions
{
    public class RenewSubscriptionDto
    {
        public Guid SubscriptionId { get; set; }

        public decimal Price { get; set; }

        public BillingInterval BillingInterval { get; set; }
    }
}
