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

        readonly ISubscriptionAppService _subscriptionAppService;

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
            var model = new RenewSubscriptionViewModel()
            {
                SubscriptionId = SubscriptionId,
                Price = Model.Price,
                BillingInterval = Model.BillingInterval,
            };

            await _subscriptionAppService.RenewSubscription(
                ObjectMapper.Map<RenewSubscriptionViewModel, RenewSubscriptionDto>(model));

            return NoContent();
        }

        public class SubscriptionViewModel
        {
            public Guid UserId { get; set; }

            public Guid PlanId { get; set; }

            public decimal Price { get; set; }

            public BillingInterval BillingInterval { get; set; }

            public DateTime StartDate { get; set; }

            public DateTime EndDate { get; set; }

            public DateTime NextBill { get; set; }

            public bool IsAutoRenew { get; set; }

            public bool IsActive { get; set; }

            public LastPaymentStatus LastPaymentStatus { get; set; }

            public SubscriptionStatus Status { get; set; }
        }

        public class RenewSubscriptionViewModel
        {
            public Guid SubscriptionId { get; set; }

            public decimal Price { get; set; }

            public BillingInterval BillingInterval { get; set; }
        }
    }
}
