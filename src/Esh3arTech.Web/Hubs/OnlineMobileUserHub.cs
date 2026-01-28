using Esh3arTech.Chats;
using Esh3arTech.Messages;
using Esh3arTech.Utility;
using Esh3arTech.Web.MessagesHandler.CacheItems;
using Esh3arTech.Web.MobileUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Caching;
using static Esh3arTech.Esh3arTechConsts;

namespace Esh3arTech.Web.Hubs
{
    [Authorize]
    [HubRoute("online-mobile-user")]
    public class OnlineMobileUserHub : Esh3arTechHubBase
    {
        private readonly OnlineUserTrackerService _onlineUserTrackerService;
        private readonly IMessageAppService _messageAppService;
        private readonly IMessageStatusUpdater _messageStatusUpdater;
        private readonly IDistributedCache<UserPendingMessageItem> _cache;
        private readonly IHubContext<BusinessUserHub> _businessHubContext;
        private readonly IChatService _chatService;

        public OnlineMobileUserHub(
            OnlineUserTrackerService onlineUserTrackerService,
            IMessageAppService messageAppService,
            IMessageStatusUpdater messageStatusUpdater,
            IDistributedCache<UserPendingMessageItem> cache,
            IHubContext<BusinessUserHub> businessHubContext,
            IChatService chatService)
        {
            _onlineUserTrackerService = onlineUserTrackerService;
            _messageAppService = messageAppService;
            _cache = cache;
            _messageStatusUpdater = messageStatusUpdater;
            _businessHubContext = businessHubContext;
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var mobileNumber = (string?)GetUserInfo();
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
                var cacheKey = mobileNumber;
                var cacheItem = await _cache.GetAsync(cacheKey);
                if (cacheItem != null && cacheItem.penddingMessages.Count > 0)
                {
                    IReadOnlyList<SendMessageModel> pendingMessages;
                    List<SendMessageModel> tempList = new();
                    foreach (var msg in cacheItem.penddingMessages)
                    {
                        tempList.Add(JsonSerializer.Deserialize<SendMessageModel>(msg)!);
                    }

                    pendingMessages = tempList;
                    await Clients.Caller.SendAsync(HubMethods.ReceivePendingMessages, JsonSerializer.Serialize(pendingMessages));
                    await _cache.RemoveAsync(cacheKey);
                }
                else
                {
                    var pendingMessages = await _messageAppService.GetPendingMessagesAsync(mobileNumber!);
                    if (pendingMessages.Any())
                    {
                        await Clients.Caller.SendAsync(HubMethods.ReceivePendingMessages, JsonSerializer.Serialize(pendingMessages));
                    }
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var mobileNumber = (string?)GetUserInfo();

            if (!string.IsNullOrEmpty(mobileNumber))
            {
                await _onlineUserTrackerService.RemoveConnection(mobileNumber);
            }
        }

        public async Task AcknowledgeMessage(Guid messageId)
        {
            var mobileNumber = (string?)GetUserInfo();

            if (string.IsNullOrEmpty(mobileNumber) || !CurrentUser.IsAuthenticated)
            {
                return;
            }

            await _messageStatusUpdater.SetMessageStatusToDeliveredInNewTransactionAsync(messageId);
        }

        public async Task SendMessage(ReceiveMessageModel model)
        {

            IsAuthorized(model.From);

            var createdMessage = await _chatService.CreateMobileToBusinessMessageAsync(
                new ReceiveToBusinessMessageDto
                {
                    Id = model.Id,
                    From = model.From,
                    MobileAccount = model.MobileAccount,
                    Content = model.Content
                });

            var connectionId = await _onlineUserTrackerService.GetFirstConnectionIdByPhoneNumberAsync(MobileNumberPreparator.PrepareMobileNumber(model.MobileAccount));

            if (!string.IsNullOrEmpty(connectionId))
            {
                await _businessHubContext.Clients.Client(connectionId).SendAsync(HubMethods.ReceiveChatMessage, JsonSerializer.Serialize(createdMessage));
            }
        }

        protected override object? GetUserInfo()
        {
            var mobileNumber = Context.User?.FindFirst(ClaimTypesConsts.MobileNumber)?.Value;
            if (!string.IsNullOrEmpty(mobileNumber))
            {
                return mobileNumber;
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

        public class SendMessageModel : EntityDto<Guid>
        {
            public string RecipientPhoneNumber { get; set; }
            public string MessageContent { get; set; }
            public string From { get; set; }
            public string AccessUrl { get; set; }
            public DateTime? UrlExpiresAt { get; set; }

        }

        public class ReceiveMessageModel
        {
            public Guid Id { get; set; }

            public string From { get; set; }

            public string MobileAccount { get; set; }

            public string Content { get; set; }
        }
    }
}
