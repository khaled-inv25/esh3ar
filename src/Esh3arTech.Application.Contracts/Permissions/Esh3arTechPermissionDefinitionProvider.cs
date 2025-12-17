using Esh3arTech.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Esh3arTech.Permissions;

public class Esh3arTechPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var esh3arTechGroup = context.AddGroup(Esh3arTechPermissions.Esh3arTechGroupName, L("Permission:Esh3arTech"));
        esh3arTechGroup.AddPermission(Esh3arTechPermissions.Esh3arSendMessages, L("Permission:SenderSendMessage"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<Esh3arTechResource>(name);
    }
}
