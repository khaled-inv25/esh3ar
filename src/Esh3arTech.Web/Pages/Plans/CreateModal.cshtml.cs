using Esh3arTech.UserPlans.Plans;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esh3arTech.Web.Pages.Plans
{
    public class CreatePlanModalModel : Esh3arTechPageModel
    {
        [BindProperty]
        public CreatePlanDto Plan { get; set; }

        public List<SelectListItem> PlanList { get; set; }

        private readonly IPlanAppService _planAppService;
        public CreatePlanModalModel(IPlanAppService planAppService)
        {
            _planAppService = planAppService;
        }

        public async Task OnGetAsync()
        {
            Plan = new CreatePlanDto();
            Plan.Features = await _planAppService.GetDefaultFeaturesAsync();

            // To load the dropdown list.
            var planLookup = await _planAppService.GetPlanLookupAsync();
            PlanList = planLookup.Select(p => new SelectListItem(p.DisplayName, p.Id.ToString())).ToList();

            // To set default value in the drowpdown list.
            PlanList.Insert(0, new SelectListItem { Value = "", Text = L["DdList:DefaultExpirePlan"], Selected = true });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _planAppService.CreatePlanAsync(Plan);

            return NoContent();
        }
    }
}
