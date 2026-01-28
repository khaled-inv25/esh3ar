using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.Widgets;

namespace Esh3arTech.Web.Pages.Bots.Components.BotManagementAdminArea
{
    [Authorize]
    [Widget(
        ScriptFiles = new[] { "/Pages/Bots/Components/BotManagementAdminArea/bot-admin-area.js" }
    )]
    public class BotManagementAdminAreaViewComponent : AbpViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View("~/Pages/Bots/Components/BotManagementAdminArea/Default.cshtml");
        }
    }
}
