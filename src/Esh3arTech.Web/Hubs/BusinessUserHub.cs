using Esh3arTech.Chats;
using Esh3arTech.Utility;
using Esh3arTech.Web.MobileUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Security.Claims;
using static Esh3arTech.Esh3arTechConsts;

namespace Esh3arTech.Web.Hubs
{
    [Authorize]
    [HubRoute("business-user")]
    public class BusinessUserHub : Esh3arTechHubBase
    {
        private readonly OnlineUserTrackerService _onlineUserTrackerService;
        private readonly IHubContext<OnlineMobileUserHub> _mobileHubContext;
        private readonly IChatService _chatService;

        public BusinessUserHub(
            OnlineUserTrackerService onlineUserTrackerService,
            IHubContext<OnlineMobileUserHub> mobileHubContext,
            IChatService chatService)
        {
            _onlineUserTrackerService = onlineUserTrackerService;
            _mobileHubContext = mobileHubContext;
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var mobileNumber = (string?)GetUserInfo();
            var connectionId = Context.ConnectionId;

            if (!string.IsNullOrEmpty(mobileNumber))
            {
                if (!string.IsNullOrEmpty(await _onlineUserTrackerService.GetFirstConnectionIdByPhoneNumberAsync(mobileNumber)))
                {
                    throw new UserFriendlyException("User is already online!");
                }
            }

            await _onlineUserTrackerService.AddConnection(mobileNumber, connectionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var mobileNumber = (string?)GetUserInfo();

            if (!string.IsNullOrEmpty(mobileNumber))
            {
                await _onlineUserTrackerService.RemoveConnection(mobileNumber);
            }
        }

        public async Task SendMessage(ReceiveMessageModel model)
        {
            IsAuthorized(model.MobileAccount);

            var createdMessage = await _chatService.CreateBusinessToMobileMessageAsync(
                new ReceiveToMobileMessageDto
                {
                    SenderId = CurrentUser.Id!.Value,
                    MessageId = model.MessageId,
                    ReceipientMobileNumber = model.ReceipientMobileNumber,
                    MobileAccount = model.MobileAccount,
                    Content = model.Content
                });

            var connectionId = await _onlineUserTrackerService.GetFirstConnectionIdByPhoneNumberAsync(MobileNumberPreparator.PrepareMobileNumber(model.ReceipientMobileNumber));

            if (!string.IsNullOrEmpty(connectionId))
            {
                await _mobileHubContext.Clients.Client(connectionId).SendAsync(HubMethods.ReceiveChatMessage, JsonSerializer.Serialize(createdMessage));
            }
        }

        protected override object? GetUserInfo()
        {
            var userId = Context.User?.FindFirst(AbpClaimTypes.PhoneNumber)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                return userId;
            }

            return null;
        }

        protected override void IsAuthorized(string number)
        {
            var currentMobileNumber = (string?)GetUserInfo();
            var mobileSender = MobileNumberPreparator.PrepareMobileNumber(number);

            if (!currentMobileNumber!.Equals(mobileSender))
            {
                throw new BusinessException("You are not authorized to send messages from this mobile number!");
            }
        }

        public class ReceiveMessageModel
        {
            public Guid SenderId { get; set; }

            public Guid MessageId { get; set; }

            public string Content { get; set; }

            public string MobileAccount { get; set; }

            public string ReceipientMobileNumber { get; set; }
        }
    }
}
