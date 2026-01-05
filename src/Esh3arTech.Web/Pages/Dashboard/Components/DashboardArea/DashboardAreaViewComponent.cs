using Esh3arTech.UserMessages;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;

namespace Esh3arTech.Web.Pages.Dashboard.Components.DashboardArea
{
    public class DashboardAreaViewComponent : AbpViewComponent
    {
        private readonly IUserMessagesAppService _userMessagesAppService;

        public DashboardAreaViewComponent(IUserMessagesAppService userMessagesAppService)
        {
            _userMessagesAppService = userMessagesAppService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = ObjectMapper.Map<MessagesStatusDto, MessagesStatusAreaViewComponentModel>(await _userMessagesAppService.GetMessagesStatus());

            return View("~/Pages/Dashboard/Components/DashboardArea/Default.cshtml", model);
        }

        public class MessagesStatusAreaViewComponentModel
        {
            public int TotalMessages { get; set; }
            public int TotalMessageThisMonth { get; set; }
            public int TotalMessageThisWeek { get; set; }
            public int TotalMessageToday { get; set; }
        }
    }
}
