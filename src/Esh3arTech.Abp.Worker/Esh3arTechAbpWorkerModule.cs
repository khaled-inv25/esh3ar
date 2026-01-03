using Volo.Abp.BackgroundWorkers.Quartz;
using Volo.Abp.Modularity;

namespace Esh3arTech.Abp.Worker
{
    [DependsOn(
    typeof(AbpBackgroundWorkersQuartzModule)
    )]
    public class Esh3arTechAbpWorkerModule : AbpModule
    {
    }
}
