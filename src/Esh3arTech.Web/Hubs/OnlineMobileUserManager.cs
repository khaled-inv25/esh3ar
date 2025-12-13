using Esh3arTech.Web.MobileUsers;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;

namespace Esh3arTech.Web.Hubs
{
    [HubRoute("online-mobile-user")]
    public class OnlineMobileUserManager : AbpHub
    {
        private readonly OnlineUserTrackerService _userTracker;

        public OnlineMobileUserManager(OnlineUserTrackerService userTracker)
        {
            _userTracker = userTracker;
        }

        public override async Task OnConnectedAsync()
        {
            var mobileNumber = Context.User?.FindFirst("mobile_number")?.Value;

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

    }
}
