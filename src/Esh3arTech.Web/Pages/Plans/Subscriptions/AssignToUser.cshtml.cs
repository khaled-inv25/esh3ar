using Esh3arTech.Plans;
using Esh3arTech.Plans.Subscriptions;
using Esh3arTech.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Esh3arTech.Web.Pages.Plans.Subscriptions
{
    public class AssignToUserModel : Esh3arTechPageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid PlanId { get; set; }

        [BindProperty]
        public SubscriptionToUserViewModel Model { get; set; }

        public PlanViewModel PlanModel { get; set; }

        public List<SelectListItem> UserList { get; set; }

        private readonly IUserAppService _userAppService;
        private readonly ISubscriptionAppService _subscriptionAppService;
        private readonly IPlanAppService _planAppService;

        public AssignToUserModel(
            IUserAppService userAppService,
            ISubscriptionAppService subscriptionAppService,
            IPlanAppService planAppService)
        {
            _userAppService = userAppService;
            _subscriptionAppService = subscriptionAppService;
            _planAppService = planAppService;
            Model = new();
        }

        public async Task OnGetAsync()
        {
            var usersLookup = await _userAppService.GetUserLookup();
            PlanModel = ObjectMapper.Map<PlanDto, PlanViewModel>(await _planAppService.GetPlanByIdAsync(PlanId));

            UserList = usersLookup.Select(u => new SelectListItem(u.UserName, u.Id.ToString())).ToList();
            UserList.Insert(0, new SelectListItem { Value = "", Text = "Select User"});

            Model.Price = PlanModel.DailyPrice;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var assginToUser = ObjectMapper.Map<SubscriptionToUserViewModel, AssignSubscriptionToUserDto>(Model);
            assginToUser.PlanId = PlanId;

            await _subscriptionAppService.AssignSubscriptionToUser(assginToUser);

            return NoContent();
        }

        public class SubscriptionToUserViewModel
        {
            [Required]
            public Guid UserId { get; set; }

            [Required]
            public Guid PlanId { get; set; }

            [AllowNull]
            public decimal? Price { get; set; }

            [Required]
            public BillingInterval BillingInterval { get; set; }

            [Required]
            public bool IsAutoRenew { get; set; }

            [Required]
            public bool IsActive { get; set; }

            public string PaymentMethod { get; set; }
        }

        public class PlanViewModel
        {
            public string Name { get; set; }

            public decimal? DailyPrice { get; set; }

            public decimal? WeeklyPrice { get; set; }

            public decimal? MonthlayPrice { get; set; }

            public decimal? AnnualPrice { get; set; }
        }

    }
}
