using Esh3arTech.Messages;
using Esh3arTech.Messages.Delivery;
using Esh3arTech.Messages.RetryPolisy;
using Esh3arTech.Web.Hubs;
using Esh3arTech.Web.MessagesHandler.CacheItems;
using Esh3arTech.Web.MobileUsers;
using Esh3arTech.Web.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;
using static Esh3arTech.Esh3arTechConsts;

namespace Esh3arTech.Web.MessagesHandler
{
    public class MessageDeliveryService : IMessageDeliveryService, ITransientDependency
    {
        #region Fields

        private readonly IMessageAppService _messageAppService;
        private readonly OnlineUserTrackerService _onlineUserTrackerService;
        private readonly IMessageStatusUpdater _messageStatusUpdater;
        private readonly IHubContext<OnlineMobileUserHub> _hubContext;
        private readonly IDistributedCache<UserPendingMessageItem> _cache;
        private readonly IRetryPolisyService _retryPolisyService;
        private readonly IObjectMapper<DeliverMessageDto, MessageModel> _objectMapper;

        #endregion

        #region Ctor

        public MessageDeliveryService(
            IMessageAppService messageAppService,
            OnlineUserTrackerService onlineUserTrackerService,
            IMessageStatusUpdater messageStatusUpdater,
            IHubContext<OnlineMobileUserHub> hubContext,
            IDistributedCache<UserPendingMessageItem> cache,
            IRetryPolisyService retryPolisyService,
            ICurrentUser currentUser,
            IObjectMapper<DeliverMessageDto, MessageModel> objectMapper)
        {
            _messageAppService = messageAppService;
            _onlineUserTrackerService = onlineUserTrackerService;
            _messageStatusUpdater = messageStatusUpdater;
            _hubContext = hubContext;
            _cache = cache;
            _retryPolisyService = retryPolisyService;
            _objectMapper = objectMapper;
        }

        #endregion

        #region Methods

        public async Task DeliverBatchMessageAsync(DeliverBatchMessageDto dtos)
        {
            var messages = JsonSerializer.Deserialize<List<MessageModel>>(dtos.JsonMessages);
            foreach(var msg in messages!)
            {
                // it's for Idempotency Guard
                if (await _messageAppService.IsMessageDeliveredOrSentAsync(msg.Id))
                {
                    return;
                }

                try
                {
                    await ProcessDeliverMessageAsync(msg);
                }
                catch (Exception ex)
                {
                    var message = (Message)await _messageAppService.GetMessageById(msg.Id);
                    await HandleMessageDeliveryFailureAsync(ex, message);
                }
            }
        }

        public async Task DeliverMessageAsync(DeliverMessageDto dto)
        {
            var msg = _objectMapper.Map(dto);

            // it's for Idempotency Guard
            if (await _messageAppService.IsMessageDeliveredOrSentAsync(msg.Id))
            {
                return;
            }

            try
            {
                await ProcessDeliverMessageAsync(msg);
            }
            catch (Exception ex)
            {
                var message = (Message)await _messageAppService.GetMessageById(msg.Id);
                await HandleMessageDeliveryFailureAsync(ex, message);
            }
        }

        private async Task ProcessDeliverMessageAsync(MessageModel model)
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
                cacheItem ??= new UserPendingMessageItem(); // to initialize it, if cacheItem is null.
                cacheItem.penddingMessages.Add(JsonSerializer.Serialize(model));
                await _cache.SetAsync(cacheKey, cacheItem);
            }
        }

        private async Task HandleMessageDeliveryFailureAsync(Exception ex, Message message)
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

        #endregion
    }
}
