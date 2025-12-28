using Esh3arTech.Messages;
using Esh3arTech.Messages.Eto;
using Esh3arTech.Messages.Idempotency;
using Esh3arTech.Messages.RetryPolicy;
using Esh3arTech.Web.Hubs;
using Esh3arTech.Web.MessagesHandler.CacheItems;
using Esh3arTech.Web.MobileUsers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
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
        private readonly IIdempotencyService _idempotencyService;
        private readonly IDistributedCache<UserPendingMessageItem> _cache;
        private readonly MessageReliabilityOptions _options;
        private readonly IMessageRetryPolicy _retryPolicy;

        public MessagesDeliveryHandler(
            IHubContext<OnlineMobileUserHub> hubContext,
            OnlineUserTrackerService onlineUserTrackerService,
            IMessageAppService messageAppService,
            IIdempotencyService idempotencyService,
            IDistributedCache<UserPendingMessageItem> cache,
            IOptions<MessageReliabilityOptions> options,
            IMessageRetryPolicy retryPolicy)
        {
            _hubContext = hubContext;
            _onlineUserTrackerService = onlineUserTrackerService;
            _messageAppService = messageAppService;
            _idempotencyService = idempotencyService;
            _cache = cache;
            _options = options.Value;
            _retryPolicy = retryPolicy;
            //_options = options;
        }

        //private readonly ICircuitBreaker _circuitBreaker;
        //private readonly IMessageRetryPolicy _retryPolicy;
        //private readonly IMessageMetricsCollector _metricsCollector;


        //public MessagesDeliveryHandler(
        //    IHubContext<OnlineMobileUserHub> hubContext,
        //    OnlineUserTrackerService onlineUserTrackerService,
        //    IDistributedCache<UserPendingMessageItem> distributedCache,
        //    IMessageAppService messageAppService,
        //    IIdempotencyService idempotencyService,
        //    ICircuitBreaker circuitBreaker,
        //    IMessageRetryPolicy retryPolicy,
        //    IMessageMetricsCollector metricsCollector,
        //    IRepository<Message, Guid> messageRepository,
        //    IOptions<MessageReliabilityOptions> options)
        //{
        //    _hubContext = hubContext;
        //    _onlineUserTrackerService = onlineUserTrackerService;
        //    _cache = distributedCache;
        //    _messageAppService = messageAppService;
        //    _idempotencyService = idempotencyService;
        //    _circuitBreaker = circuitBreaker;
        //    _retryPolicy = retryPolicy;
        //    _metricsCollector = metricsCollector;
        //    _messageRepository = messageRepository;
        //    _options = options.Value;
        //}

        public async Task HandleEventAsync(SendOneWayMessageEto eventData)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (!string.IsNullOrEmpty(eventData.IdempotencyKey))
                {
                    if (await _idempotencyService.IsProcessedAsync(eventData.IdempotencyKey))
                    {
                        //_logger.LogInformation("Message {MessageId} already processed (idempotency key: {IdempotencyKey})", 
                        //eventData.Id, eventData.IdempotencyKey);
                        return;
                    }
                }

                // 2. Check circuit breaker
                //if (await _circuitBreaker.IsOpenAsync())
                //{
                //    //_logger.LogWarning("Circuit breaker is open, scheduling retry for message {MessageId}", eventData.Id);
                //    //await ScheduleRetryAsync(eventData);
                //    return;
                //}

                var currentMessageStatus = await _messageAppService.GetMessageStatusById(eventData.Id);

                if (currentMessageStatus.Equals(MessageStatus.Sent) ||
                    currentMessageStatus.Equals(MessageStatus.Pending) ||
                    currentMessageStatus.Equals(MessageStatus.Delivered))
                {
                    return;
                }

                await DeliverMessageAsync(eventData);

                // 5. Record success
                //await _circuitBreaker.RecordSuccessAsync();
                //await _metricsCollector.RecordMessageProcessedAsync();

                if (!string.IsNullOrEmpty(eventData.IdempotencyKey))
                {
                    await _idempotencyService.MarkAsProcessedAsync(
                        eventData.IdempotencyKey,
                        TimeSpan.FromHours(_options.IdempotencyTtlHours));
                }

                stopwatch.Stop();
                //await _metricsCollector.RecordProcessingTimeAsync(stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                //stopwatch.Stop();
                //await _circuitBreaker.RecordFailureAsync();
                //await _metricsCollector.RecordMessageFailedAsync();
                await HandleDeliveryFailureAsync(eventData, ex);
            }
        }

        private async Task DeliverMessageAsync(SendOneWayMessageEto eto)
        {
            throw new Exception("Dum dum exception...");
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

        private async Task HandleDeliveryFailureAsync(SendOneWayMessageEto eventData, Exception exception)
        {
            //_logger.LogError(exception, "Failed to deliver message {MessageId}", eventData.Id);

            try
            {
                var message = await _messageAppService.GetMessageById(eventData.Id) as Message;
                // Increment retry count
                message!.IncrementRetryCount();

                // Check if we can retry
                if (message.CanRetry(_retryPolicy.MaxRetries))
                {
                    // Schedule retry with exponential backoff
                    var delay = _retryPolicy.CalculateDelay(message.RetryCount);
                    message.ScheduleNextRetry(delay);
                    message.SetMessageStatusType(MessageStatus.Failed);
                    message.SetFailureReason(exception.Message);

                    //await _messageAppService.UpdateAsync(message, autoSave: true);

                    //_logger.LogInformation("Scheduled retry {RetryCount} for message {MessageId} in {Delay} seconds", 
                    //message.RetryCount, eventData.Id, delay.TotalSeconds);
                }
                else
                {
                    // Max retries reached, move to failed status
                    message.SetMessageStatusType(MessageStatus.Failed);
                    message.SetFailureReason($"Max retries reached. Last error: {exception.Message}");

                    //await _messageRepository.UpdateAsync(message, autoSave: true);

                    //_logger.LogWarning("Message {MessageId} failed after {RetryCount} retries", 
                    //eventData.Id, message.RetryCount);
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error handling delivery failure for message {MessageId}", eventData.Id);
            }
        }

        //private async Task ScheduleRetryAsync(SendOneWayMessageEto eventData)
        //{
        //    try
        //    {
        //        var message = await _messageRepository.GetAsync(eventData.Id);

        //        if (message.CanRetry(_retryPolicy.MaxRetries))
        //        {
        //            var delay = _retryPolicy.CalculateDelay(message.RetryCount);
        //            message.ScheduleNextRetry(delay);
        //            message.MarkAsRetrying();

        //            await _messageRepository.UpdateAsync(message, autoSave: true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //_logger.LogError(ex, "Error scheduling retry for message {MessageId}", eventData.Id);
        //    }
        //}


        private async Task UpdateMessageStatusAsync(Guid id, MessageStatus status)
        {
            await _messageAppService.UpdateMessageStatus(new UpdateMessageStatusDto { Id = id, Status = status });
        }
    }
}
