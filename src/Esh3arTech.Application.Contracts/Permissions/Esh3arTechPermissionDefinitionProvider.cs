using Esh3arTech.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Esh3arTech.Permissions;

public class Esh3arTechPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(Esh3arTechPermissions.GroupName);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(Esh3arTechPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<Esh3arTechResource>(name);
    }
}
