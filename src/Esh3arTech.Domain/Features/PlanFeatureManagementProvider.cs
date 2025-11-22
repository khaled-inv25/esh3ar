using Volo.Abp.DependencyInjection;
using Volo.Abp.FeatureManagement;

namespace Esh3arTech.Features
{
    public class PlanFeatureManagementProvider : FeatureManagementProvider, ITransientDependency
    {
        public override string Name => "P"; // For Plan.

        public PlanFeatureManagementProvider(IFeatureManagementStore store) : base(store)
        {
        }
    }
}
