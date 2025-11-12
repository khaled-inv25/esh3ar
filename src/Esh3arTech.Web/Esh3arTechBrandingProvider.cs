using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Localization;
using Esh3arTech.Localization;

namespace Esh3arTech.Web;

[Dependency(ReplaceServices = true)]
public class Esh3arTechBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<Esh3arTechResource> _localizer;

    public Esh3arTechBrandingProvider(IStringLocalizer<Esh3arTechResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
