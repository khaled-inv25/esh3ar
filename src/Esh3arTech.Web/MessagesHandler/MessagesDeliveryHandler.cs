using Esh3arTech.Messages;
using Esh3arTech.Messages.Eto;
using Esh3arTech.Messages.RetryPolisy;
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
        private readonly IDistributedCache<UserPendingMessageItem> _cache;
        private readonly IMessageAppService _messageAppService;
        private readonly IMessageStatusUpdater _messageStatusUpdater;
        private readonly IRetryPolisyService _retryPolisyService;

        public MessagesDeliveryHandler(
            IHubContext<OnlineMobileUserHub> hubContext,
            OnlineUserTrackerService onlineUserTrackerService,
            IDistributedCache<UserPendingMessageItem> distributedCache,
            IMessageAppService messageAppService,
            IMessageStatusUpdater messageStatusUpdater,
            IRetryPolisyService retryPolisyService)
        {
            _hubContext = hubContext;
            _onlineUserTrackerService = onlineUserTrackerService;
            _cache = distributedCache;
            _messageAppService = messageAppService;
            _retryPolisyService = retryPolisyService;
            _messageStatusUpdater = messageStatusUpdater;
        }

        public async Task HandleEventAsync(SendOneWayMessageEto eventData)
        {
            var message = (Message) await _messageAppService.GetMessageById(eventData.Id);

            // it's for Idempotency Guard
            if (message.Status.Equals(MessageStatus.Delivered) || message.Status.Equals(MessageStatus.Sent))
            {
                return;
            }
            
            try
            {
                await DeliverMessageAsync(eventData);
            }
            catch (Exception ex)
            {
                await HandleMessageDeliveryFailureAsync(eventData, ex, message);
            }
        }

        private async Task DeliverMessageAsync(SendOneWayMessageEto eto)
        {
            var connectionId = await _onlineUserTrackerService.GetFirstConnectionIdByPhoneNumberAsync(eto.RecipientPhoneNumber);

            // To send message if user online other ways save it in db and cache as pending message.
            if (!string.IsNullOrEmpty(connectionId))
            {
                await _messageStatusUpdater.SetMessageStatusToSentInNewTransactionAsync(eto.Id);
                await _hubContext.Clients.Client(connectionId).SendAsync(HubMethods.ReceiveMessage, JsonSerializer.Serialize(eto));
            }
            else
            {
                await _messageStatusUpdater.SetMessageStatusToPendingInNewTransactionAsync(eto.Id);

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

        private async Task HandleMessageDeliveryFailureAsync(SendOneWayMessageEto eto, Exception ex, Message message)
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
}
