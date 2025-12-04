using System;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.Plans.Subscriptions
{
    public class SubscriptionDto : EntityDto
    {
        public Guid UserId { get; private set; }

        public Guid PlanId { get; private set; }

        public decimal Price { get; private set; }

        public BillingInterval BillingInterval { get; private set; }

        public DateTime StartDate { get; private set; }

        public DateTime EndDate { get; private set; }

        public DateTime NextBill { get; private set; }

        public bool IsAutoRenew { get; private set; }

        public bool IsActive { get; private set; }

        public LastPaymentStatus LastPaymentStatus { get; set; }

        public SubscriptionStatus Status { get; set; }
    }
}
