using Esh3arTech.Messages;
using Esh3arTech.Messages.Eto;
using Esh3arTech.Messages.RetryPolisy;
using Esh3arTech.Web.Hubs;
using Esh3arTech.Web.MessagesHandler.CacheItems;
using Esh3arTech.Web.MobileUsers;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using static Esh3arTech.Esh3arTechConsts;

namespace Esh3arTech.Web.MessagesHandler
{
    public class MultipleMessageDelivaryHandler : IDistributedEventHandler<MessageIngestionEto>, ITransientDependency
    {
        private readonly IHubContext<OnlineMobileUserHub> _hubContext;
        private readonly OnlineUserTrackerService _onlineUserTrackerService;
        private readonly IDistributedCache<UserPendingMessageItem> _cache;
        private readonly IMessageAppService _messageAppService;
        private readonly IMessageStatusUpdater _messageStatusUpdater;
        private readonly IRetryPolisyService _retryPolisyService;

        public MultipleMessageDelivaryHandler(
            IHubContext<OnlineMobileUserHub> hubContext,
            OnlineUserTrackerService onlineUserTrackerService, 
            IDistributedCache<UserPendingMessageItem> cache, 
            IMessageAppService messageAppService, 
            IMessageStatusUpdater messageStatusUpdater, 
            IRetryPolisyService retryPolisyService)
        {
            _hubContext = hubContext;
            _onlineUserTrackerService = onlineUserTrackerService;
            _cache = cache;
            _messageAppService = messageAppService;
            _messageStatusUpdater = messageStatusUpdater;
            _retryPolisyService = retryPolisyService;
        }

        public async Task HandleEventAsync(MessageIngestionEto eventData)
        {
            var messages = JsonSerializer.Deserialize<List<MessageModel>>(eventData.JsonMessages);

            foreach (var msg in messages!)
            {
                var message = (Message) await _messageAppService.GetMessageById(msg.Id);

                // it's for Idempotency Guard
                if (message.Status.Equals(MessageStatus.Delivered) || message.Status.Equals(MessageStatus.Sent))
                {
                    return;
                }

                try
                {
                    await DeliverMessageAsync(msg);
                }
                catch (Exception ex)
                {
                    await HandleMessageDeliveryFailureAsync(msg, ex, message);
                }
            }
        }

        private async Task DeliverMessageAsync(MessageModel model)
        {
            var connectionId = await _onlineUserTrackerService.GetFirstConnectionIdByPhoneNumberAsync(model.RecipientPhoneNumber);

            // To send message if user online other ways save it in db and cache as pending message.
            if (!string.IsNullOrEmpty(connectionId))
            {
                await _messageStatusUpdater.SetMessageStatusToSentInNewTransactionAsync(model.Id);
                await _hubContext.Clients.Client(connectionId).SendAsync(HubMethods.ReceiveMessage, JsonSerializer.Serialize(model));
            }
            else
            {
                await _messageStatusUpdater.SetMessageStatusToPendingInNewTransactionAsync(model.Id);

                var cacheKey = model.RecipientPhoneNumber;
                var cacheItem = await _cache.GetAsync(cacheKey);
                if (cacheItem == null)
                {
                    cacheItem = new UserPendingMessageItem();
                }
                cacheItem.penddingMessages.Add(JsonSerializer.Serialize(model));

                await _cache.SetAsync(cacheKey, cacheItem);
            }
        }

        private async Task HandleMessageDeliveryFailureAsync(MessageModel model, Exception ex, Message message)
        {
            message.IncrementRetryCount();

            if (_retryPolisyService.CanRetry(message.RetryCount))
            {
                var delay = _retryPolisyService.CalculateDelay(message.RetryCount);
                message.ScheduleNextRetry(delay);
                message.SetMessageStatusType(MessageStatus.Failed);
                message.SetFailureReason(ex.Message);
            }
            else
            {
                message.SetMessageStatusType(MessageStatus.Failed);
                message.MarkAsPermanentlyFailed();
                message.SetFailureReason($"Max retries reached. Last error: {ex.Message}");
            }

            await _messageAppService.UpdateMessage(message);
        }
    }

    public class MessageModel
    {
        public Guid Id { get; set; }

        public string RecipientPhoneNumber { get; set; }

        public string Subject { get; set; }

        public string? MessageContent { get; set; }

        public MessageStatus Status { get; set; }

        public MessageType Type { get; set; }
    }
}
