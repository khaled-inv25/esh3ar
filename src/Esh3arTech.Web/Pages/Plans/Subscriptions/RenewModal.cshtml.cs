using Esh3arTech.Plans.Subscriptions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Esh3arTech.Web.Pages.Plans.Subscriptions
{
    public class RenewModalModel : Esh3arTechPageModel
    {
        [BindProperty]
        public SubscriptionViewModel Model { get; set; }

        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid SubscriptionId { get; set; }

        private readonly ISubscriptionAppService _subscriptionAppService;

        public RenewModalModel(ISubscriptionAppService subscriptionAppService)
        {
            _subscriptionAppService = subscriptionAppService;
        }

        public async Task OnGetAsync()
        {
            Model = ObjectMapper.Map<SubscriptionDto, SubscriptionViewModel>(await _subscriptionAppService.GetSubscriptionByIdAsync(SubscriptionId));
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _subscriptionAppService.RenewSubscription(
                new RenewSubscriptionDto
                {
                    SubscriptionId = SubscriptionId,
                    Price = Model.Price,
                    BillingInterval = Model.BillingInterval,
                });

            return NoContent();
        }

        public class SubscriptionViewModel
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
}
