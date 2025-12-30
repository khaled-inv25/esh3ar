using Esh3arTech.Messages;
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
    public class OnlineMobileUserHub : AbpHub
    {
        private readonly OnlineUserTrackerService _onlineUserTrackerService;
        private readonly IMessageAppService _messageAppService;
        private readonly IMessageStatusUpdater _messageStatusUpdater;
        private readonly IDistributedCache<UserPendingMessageItem> _cache;

        public OnlineMobileUserHub(
            OnlineUserTrackerService onlineUserTrackerService,
            IMessageAppService messageAppService,
            IMessageStatusUpdater messageStatusUpdater,
            IDistributedCache<UserPendingMessageItem> cache)
        {
            _onlineUserTrackerService = onlineUserTrackerService;
            _messageAppService = messageAppService;
            _cache = cache;
            _messageStatusUpdater = messageStatusUpdater;
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

            await _messageStatusUpdater.SetMessageStatusToDeliveredInNewTransactionAsync(messageId);
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

        public class SendMessageModel : EntityDto<Guid>
        {
            public string RecipientPhoneNumber { get; set; }
            public string MessageContent { get; set; }
            public string From { get; set; }
            public string AccessUrl { get; set; }
            public DateTime? UrlExpiresAt { get; set; }

        }
    }
}
