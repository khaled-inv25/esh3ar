using System;
using System.ComponentModel.DataAnnotations;

namespace Esh3arTech.Plans.Subscriptions
{
    public class AssignSubscriptionToUserDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid PlanId { get; set; }

        [Required]
        public BillingInterval BillingInterval { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public bool IsAutoRenew { get; set; }

        [Required]
        public bool IsActive { get; set; }

    }
}
