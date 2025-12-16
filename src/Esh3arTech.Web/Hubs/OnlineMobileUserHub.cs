using Esh3arTech.Messages;
using Esh3arTech.Web.MobileUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;

namespace Esh3arTech.Web.Hubs
{
    [Authorize]
    [HubRoute("online-mobile-user")]
    public class OnlineMobileUserHub : AbpHub
    {
        private readonly OnlineUserTrackerService _onlineUserTrackerService;
        private readonly IMessageAppService _messageAppService;

        public OnlineMobileUserHub(
            OnlineUserTrackerService onlineUserTrackerService, 
            IMessageAppService messageAppService)
        {
            _onlineUserTrackerService = onlineUserTrackerService;
            _messageAppService = messageAppService;
        }

        public override async Task OnConnectedAsync()
        {
            var mobileNumber = GetMobileNumber();
            var connectionId = Context.ConnectionId;

            if (!string.IsNullOrEmpty(mobileNumber))
            {
                _onlineUserTrackerService.AddConnection(mobileNumber, connectionId);

                // Check if any pending messages.
                var pendingMessages = await _messageAppService.GetPendingMessagesAsync(mobileNumber!);
                if (pendingMessages.Any())
                {
                    await Clients.Caller.SendAsync("ReceivePendingMessages", JsonSerializer.Serialize(pendingMessages));
                }
            }

            await Task.CompletedTask;
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

        public async Task AcknowledgeMessage(Guid messageId)
        {
            var mobileNumber = GetMobileNumber();

            if (string.IsNullOrEmpty(mobileNumber) || !CurrentUser.IsAuthenticated)
            {
                return;
            }

            await _messageAppService.UpdateMessageStatusToDeliveredAsync(messageId);
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
