using Esh3arTech.Messages;
using Esh3arTech.Messages.Eto;
using Esh3arTech.Web.Hubs;
using Esh3arTech.Web.MessagesHandler.CacheItems;
using Esh3arTech.Web.MobileUsers;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using static Esh3arTech.Esh3arTechConsts;

namespace Esh3arTech.Web.MessagesHandler
{
    public class MessagesDeliveryHandler : IDistributedEventHandler<SendOneWayMessageEto>, ITransientDependency
    {
        private readonly IHubContext<OnlineMobileUserHub> _hubContext;
        private readonly OnlineUserTrackerService _onlineUserTrackerService;
        private readonly IDistributedCache<UserPendingMessageItem> _cache;
        private readonly IMessageAppService _messageAppService;

        public MessagesDeliveryHandler(
            IHubContext<OnlineMobileUserHub> hubContext,
            OnlineUserTrackerService onlineUserTrackerService,
            IDistributedCache<UserPendingMessageItem> distributedCache,
            IMessageAppService messageAppService)
        {
            _hubContext = hubContext;
            _onlineUserTrackerService = onlineUserTrackerService;
            _cache = distributedCache;
            _messageAppService = messageAppService;
        }

        public async Task HandleEventAsync(SendOneWayMessageEto eventData)
        {
            await SendRealTimeOrPendMessageAsync(eventData);
        }

        private async Task SendRealTimeOrPendMessageAsync(SendOneWayMessageEto eto)
        {
            var connectionId = await _onlineUserTrackerService.GetFirstConnectionIdByPhoneNumberAsync(eto.RecipientPhoneNumber);

            // To send message if user online other ways save it in db and cache as pending message.
            if (!string.IsNullOrEmpty(connectionId))
            {
                await UpdateMessageStatusAsync(eto.Id, MessageStatus.Sent);
                await _hubContext.Clients.Client(connectionId).SendAsync(HubMethods.ReceiveMessage, JsonSerializer.Serialize(eto));
            }
            else
            {
                await UpdateMessageStatusAsync(eto.Id, MessageStatus.Pending);

                var cacheKey = eto.RecipientPhoneNumber;
                var cacheItem = await _cache.GetAsync(cacheKey);
                if (cacheItem == null)
                {
                    cacheItem = new UserPendingMessageItem();
                }
                cacheItem.penddingMessages.Add(JsonSerializer.Serialize(eto));

                await _cache.SetAsync(cacheKey, cacheItem);
            }
        }

        private async Task UpdateMessageStatusAsync(Guid id, MessageStatus status)
        {
            await _messageAppService.UpdateMessageStatus(new UpdateMessageStatusDto { Id = id, Status = status});
        }
    }
}
