using Esh3arTech.Plans;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esh3arTech.Web.Pages.Plans
{
    public class EditModalModel : Esh3arTechPageModel
    {
        #region Fields

        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public UpdatePlanDto UpdatePlanDto { get; set; }

        public List<SelectListItem> PlanList { get; set; }

        #endregion

        private readonly IPlanAppService _planAppService;

        public EditModalModel(IPlanAppService planAppService)
        {
            _planAppService = planAppService;
        }

        public async Task OnGetAsync()
        {
            var plan = await _planAppService.GetPlanByIdAsync(Id);
            UpdatePlanDto = ObjectMapper.Map<PlanDto, UpdatePlanDto>(plan);

            // To load the dropdown list.
            var planLookup = await _planAppService.GetPlanLookupAsync();
            PlanList = planLookup.Select(p => new SelectListItem(p.DisplayName, p.Id.ToString())).ToList();

            if (!plan.ExpiringPlanId.HasValue)
            {
                PlanList.Insert(0, new SelectListItem { Value = null, Text = L["DdList:DefaultExpirePlan"], Selected = true });
            }
            else
            {
                var index = PlanList.Count - 1;
                PlanList.Insert(index, new SelectListItem { Value = null, Text = L["DdList:Null"], Selected = false });
            }
        }

        public async Task<IActionResult> OnPostAsync() 
        {
            await _planAppService.UpdateAsync(Id, UpdatePlanDto);
            return NoContent();
        }
    }
}
