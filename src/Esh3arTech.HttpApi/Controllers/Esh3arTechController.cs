using Esh3arTech.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Esh3arTech.Controllers;

public abstract class Esh3arTechController : AbpControllerBase
{
    protected Esh3arTechController()
    {
        LocalizationResource = typeof(Esh3arTechResource);
    }
}
