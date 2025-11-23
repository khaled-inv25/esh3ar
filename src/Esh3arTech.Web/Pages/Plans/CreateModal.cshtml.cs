using Esh3arTech.UserPlans.Plans;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.Features;

namespace Esh3arTech.Web.Pages.Plans
{
    public class CreatePlanModalModel : Esh3arTechPageModel
    {
        [BindProperty]
        public CreatePlanDto Plan { get; set; }

        private readonly IPlanAppService _planAppService;
        private readonly IFeatureDefinitionManager _featureDefinitionManager;

        public CreatePlanModalModel(
            IPlanAppService planAppService, 
            IFeatureDefinitionManager featureDefinitionManager)
        {
            _planAppService = planAppService;
            _featureDefinitionManager = featureDefinitionManager;
        }

        public async Task OnGet()
        {
            Plan = new CreatePlanDto();
            Plan.Features = await _planAppService.GetDefaultFeaturesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _planAppService.CreatePlanAsync(Plan);

            return NoContent();
        }
    }
}
