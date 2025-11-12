using Esh3arTech.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Esh3arTech.Web.Pages;

public abstract class Esh3arTechPageModel : AbpPageModel
{
    protected Esh3arTechPageModel()
    {
        LocalizationResourceType = typeof(Esh3arTechResource);
    }
}
