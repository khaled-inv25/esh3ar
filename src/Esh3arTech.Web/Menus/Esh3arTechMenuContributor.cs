using Esh3arTech.Localization;
using Esh3arTech.MultiTenancy;
using Esh3arTech.Permissions;
using System.Threading.Tasks;
using Volo.Abp.Identity.Web.Navigation;
using Volo.Abp.SettingManagement.Web.Navigation;
using Volo.Abp.TenantManagement.Web.Navigation;
using Volo.Abp.UI.Navigation;

namespace Esh3arTech.Web.Menus;

public class Esh3arTechMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private static Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<Esh3arTechResource>();

        // Dashboard
        context.Menu.AddItem(
            new ApplicationMenuItem(
                Esh3arTechMenus.Dashboard,
                l["Menu:Dashboard"],
                "~/",
                icon: "fa fa-chart-line",
                order: 1
            )
        );

        // Plans
        context.Menu.AddItem(
            new ApplicationMenuItem(
                Esh3arTechMenus.Plans,
                l["Menu:Plans"],
                "~/Plans",
                icon: "fa-solid fa-bag-shopping",
                order: 1,
                requiredPermissionName: Esh3arTechPermissions.Esh3arPlans
            )
        );

        // Subscriptions
        context.Menu.AddItem(
                new ApplicationMenuItem(
                    Esh3arTechMenus.Subscriptions,
                    l["Menu:Subscriptions"],
                    "~/Plans/Subscriptions",
                    icon: "fa-solid fa-id-badge",
                    order: 1,
                    requiredPermissionName: Esh3arTechPermissions.Esh3arSubscriptions
                )
            );

        context.Menu.AddItem(
                new ApplicationMenuItem(
                    Esh3arTechMenus.Messages,
                    l["Menu:Messages"],
                    "~/Messages",
                    icon: "fa-solid fa-message",
                    order: 1,
                    requiredPermissionName: Esh3arTechPermissions.Esh3arSendMessages
                )
            );


        //Administration
        var administration = context.Menu.GetAdministration();
        administration.Order = 6;

        //Administration->Identity
        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 1);

        if (MultiTenancyConsts.IsEnabled)
        {
            administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        }
        else
        {
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        }

        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 3);

        //Administration->Settings
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 7);

        return Task.CompletedTask;
    }
}
