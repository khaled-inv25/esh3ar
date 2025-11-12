using Esh3arTech.Localization;
using Volo.Abp.Application.Services;

namespace Esh3arTech;

/* Inherit your application services from this class.
 */
public abstract class Esh3arTechAppService : ApplicationService
{
    protected Esh3arTechAppService()
    {
        LocalizationResource = typeof(Esh3arTechResource);
    }
}
