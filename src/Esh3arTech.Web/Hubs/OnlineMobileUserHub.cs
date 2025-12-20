using Esh3arTech.Messages;
using Esh3arTech.Web.MobileUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.SignalR;
using static Esh3arTech.Esh3arTechConsts;

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
                // To prevent multiple connections from the same mobile number.
                if (!string.IsNullOrEmpty(await _onlineUserTrackerService.GetFirstConnectionIdByPhoneNumberAsync(mobileNumber)))
                {
                    throw new UserFriendlyException("Mobile Number is already online!");
                }

                await _onlineUserTrackerService.AddConnection(mobileNumber, connectionId);

                // Check if any pending messages.
                var pendingMessages = await _messageAppService.GetPendingMessagesAsync(mobileNumber!);
                if (pendingMessages.Any())
                {
                    await Clients.Caller.SendAsync(HubMethods.ReceivePendingMessages, JsonSerializer.Serialize(pendingMessages));
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var mobileNumber = GetMobileNumber();
            var connectionId = Context.ConnectionId;

            if (!string.IsNullOrEmpty(mobileNumber))
            {
                await _onlineUserTrackerService.RemoveConnection(mobileNumber, connectionId);
            }
        }

        public async Task AcknowledgeMessage(Guid messageId)
        {
            var mobileNumber = GetMobileNumber();

            if (string.IsNullOrEmpty(mobileNumber) || !CurrentUser.IsAuthenticated)
            {
                return;
            }

            await _messageAppService.UpdateMessageStatus(new UpdateMessageStatusDto() { Id = messageId, Status = MessageStatus.Delivered });
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
