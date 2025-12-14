using Esh3arTech.Web.MobileUsers;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;

namespace Esh3arTech.Web.Hubs
{
    [Authorize]
    [HubRoute("online-mobile-user")]
    public class OnlineMobileUserHub : AbpHub
    {
        private readonly OnlineUserTrackerService _onlineUserTrackerService;

        public OnlineMobileUserHub(OnlineUserTrackerService onlineUserTrackerService)
        {
            _onlineUserTrackerService = onlineUserTrackerService;
        }

        public override async Task OnConnectedAsync()
        {            
            var mobileNumber = GetMobileNumber();
            var connectionId = Context.ConnectionId;
            
            if (!string.IsNullOrEmpty(mobileNumber))
            {
                _onlineUserTrackerService.AddConnection(mobileNumber, connectionId);
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var mobileNumber = GetMobileNumber();
            var connectionId = Context.ConnectionId;

            if (!string.IsNullOrEmpty(mobileNumber))
            {
                _onlineUserTrackerService.RemoveConnection(mobileNumber, connectionId);
            }

            return base.OnDisconnectedAsync(exception);
        }

        private string? GetMobileNumber()
        {
            var phoneNumber = Context.User?.FindFirst(ClaimTypesConsts.MobileNumber)?.Value;
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                return phoneNumber;
            }

            return null;
        }
    }
}
