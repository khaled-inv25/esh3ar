using Esh3arTech.Web.MobileUsers;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Identity;

namespace Esh3arTech.Web.Hubs
{
    [Authorize]
    [HubRoute("online-mobile-user")]
    public class OnlineMobileUserHub : AbpHub
    {
        private readonly OnlineUserTrackerService _onlineUserTrackerService;
        private readonly IIdentityUserRepository _identityUserRepository;

        public OnlineMobileUserHub(
            OnlineUserTrackerService onlineUserTrackerService, 
            IIdentityUserRepository identityUserRepository)
        {
            _onlineUserTrackerService = onlineUserTrackerService;
            _identityUserRepository = identityUserRepository;
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
            var mobilePhoneClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.MobilePhone)?.Value;
            if (!string.IsNullOrEmpty(mobilePhoneClaim))
            {
                return mobilePhoneClaim;
            }

            return Context.User?.FindFirst("phone_number")?.Value;
        }
    }
}
