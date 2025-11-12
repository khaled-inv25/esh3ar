using Esh3arTech.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Esh3arTech.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(Esh3arTechEntityFrameworkCoreModule),
    typeof(Esh3arTechApplicationContractsModule)
)]
public class Esh3arTechDbMigratorModule : AbpModule
{
}
