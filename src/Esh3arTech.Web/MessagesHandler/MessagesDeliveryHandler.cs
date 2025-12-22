using Esh3arTech.Messages;
using Esh3arTech.Web.Hubs;
using Esh3arTech.Web.MessagesHandler.CacheItems;
using Esh3arTech.Web.MobileUsers;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using static Esh3arTech.Esh3arTechConsts;

namespace Esh3arTech.Web.MessagesHandler
{
    public class MessagesDeliveryHandler : IDistributedEventHandler<SendOneWayMessageEto>, ITransientDependency
    {
        private readonly IHubContext<OnlineMobileUserHub> _hubContext;
        private readonly OnlineUserTrackerService _onlineUserTrackerService;
        private readonly IMessageAppService _messageAppService;
        private readonly IDistributedCache<UserPendingMessageItem> _cache;

        public MessagesDeliveryHandler(
            IHubContext<OnlineMobileUserHub> hubContext,
            OnlineUserTrackerService onlineUserTrackerService,
            IMessageAppService messageAppService,
            IDistributedCache<UserPendingMessageItem> distributedCache)
        {
            _hubContext = hubContext;
            _onlineUserTrackerService = onlineUserTrackerService;
            _messageAppService = messageAppService;
            _cache = distributedCache;
        }

        public async Task HandleEventAsync(SendOneWayMessageEto eventData)
        {
            await SendRealTimeOrPendMessageAsync(eventData.Id, eventData.RecipientPhoneNumber, eventData.MessageContent, eventData.From);
        }

        private async Task SendRealTimeOrPendMessageAsync(Guid id, string phoneNumber, string messageContent, string from)
        {
            var connectionId = await _onlineUserTrackerService.GetFirstConnectionIdByPhoneNumberAsync(phoneNumber);

            // To send message if user online other ways save it in db and cache as pending message.
            if (!string.IsNullOrEmpty(connectionId))
            {
                var model = new SendMessageModel
                {
                    Id = id,
                    RecipientPhoneNumber = phoneNumber,
                    MessageContent = messageContent,
                    From = from
                };

                await _hubContext.Clients.Client(connectionId).SendAsync(HubMethods.ReceiveMessage, JsonSerializer.Serialize(model));
                await _messageAppService.UpdateMessageStatus(new UpdateMessageStatusDto() { Id = id, Status = MessageStatus.Sent });
            }
            else
            {
                await _messageAppService.UpdateMessageStatus(new UpdateMessageStatusDto() { Id = id, Status = MessageStatus.Pending });

                var model = new SendMessageModel()
                {
                    Id = id,
                    RecipientPhoneNumber = phoneNumber,
                    MessageContent = messageContent,
                    From = from
                };

                var cacheKey = phoneNumber;
                var cacheItem = await _cache.GetAsync(cacheKey);
                if (cacheItem == null)
                {
                    cacheItem = new UserPendingMessageItem();
                }
                cacheItem.penddingMessages.Add(JsonSerializer.Serialize(model));

                await _cache.SetAsync(cacheKey, cacheItem);
            }
        }

        public class SendMessageModel
        {
            public Guid Id { get; set; }
            public string RecipientPhoneNumber { get; set; }
            public string MessageContent { get; set; }
            public string From { get; set; }

        }
    }
}
