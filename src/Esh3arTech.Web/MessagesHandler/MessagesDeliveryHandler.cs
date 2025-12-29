using Esh3arTech.Messages;
using Esh3arTech.Messages.Eto;
using Esh3arTech.Messages.RetryPolisy;
using Esh3arTech.Web.Hubs;
using Esh3arTech.Web.MessagesHandler.CacheItems;
using Esh3arTech.Web.MobileUsers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
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
        private readonly MessageReliabilityOptions _options;
        private readonly IRetryPolisyService _retryPolisyService;

        private const int FailedAt = 3;
        private int count = 0;


        public MessagesDeliveryHandler(
            IHubContext<OnlineMobileUserHub> hubContext,
            OnlineUserTrackerService onlineUserTrackerService,
            IDistributedCache<UserPendingMessageItem> distributedCache,
            IMessageAppService messageAppService,
            IOptions<MessageReliabilityOptions> option,
            IRetryPolisyService retryPolisyService)
        {
            _hubContext = hubContext;
            _onlineUserTrackerService = onlineUserTrackerService;
            _cache = distributedCache;
            _messageAppService = messageAppService;
            _options = option.Value;
            _retryPolisyService = retryPolisyService;
        }

        public async Task HandleEventAsync(SendOneWayMessageEto eventData)
        {
            
            try
            {
                await DeliverMessageAsync(eventData);
            }
            catch (Exception ex)
            {
                await HandleMessageDeliveryFailureAsync(eventData, ex);
            }
        }

        private async Task DeliverMessageAsync(SendOneWayMessageEto eto)
        {
            if (eto.MessageContent!.Equals("ex", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Test exception");
            }

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

        private async Task HandleMessageDeliveryFailureAsync(SendOneWayMessageEto eto, Exception ex)
        {
            var message = (Message)await _messageAppService.GetMessageById(eto.Id);
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
                message.SetFailureReason($"Max retries reached. Last error: {ex.Message}");
            }

            await _messageAppService.UpdateMessage(message);
        }

        private async Task UpdateMessageStatusAsync(Guid id, MessageStatus status)
        {
            await _messageAppService.UpdateMessageStatus(new UpdateMessageStatusDto { Id = id, Status = status});
        }
    }
}
