using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;

namespace Esh3arTech.Web.Pages.Messages.Components.SendAttachmentArea
{
    public class SendAttachmentAreaViewComponent : AbpViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(SendMessageViewModal model)
        {
            return View("~/Pages/Messages/Components/SendAttachmentArea/_imageSection.cshtml", model);
        }
    }
}
