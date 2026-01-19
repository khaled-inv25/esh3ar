using Esh3arTech.Features;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Features;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace Esh3arTech;

[DependsOn(
    typeof(Esh3arTechDomainModule),
    typeof(Esh3arTechApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class Esh3arTechApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<Esh3arTechApplicationModule>();
        });

        // To add our custom feature value provider to the beginning of the providers list.
        // Register the WRITER
        Configure<FeatureManagementOptions>(options =>
        {
            options.Providers.Insert(0, typeof(PlanFeatureManagementProvider));
        });

        // To add our custom feature value provider.
        // Register the READER
        Configure<AbpFeatureOptions>(options =>
        {
            options.ValueProviders.Add<UserPlanFeatureValueProvider>();
        });
    }
}
