//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Volo.Abp;
//using Volo.Abp.Application.Dtos;
//using Volo.Abp.Application.Services;
//using Volo.Abp.Domain.Repositories;
//using Volo.Abp.EventBus.Distributed;
//using Esh3arTech.Messages.Eto;
//using Microsoft.Extensions.Logging;

//namespace Esh3arTech.Messages
//{
//    public class DeadLetterQueueAppService : ApplicationService, IDeadLetterQueueAppService
//    {
//        private readonly IRepository<Message, Guid> _messageRepository;
//        private readonly IDistributedEventBus _distributedEventBus;
//        private readonly ILogger<DeadLetterQueueAppService> _logger;

//        public DeadLetterQueueAppService(
//            IRepository<Message, Guid> messageRepository,
//            IDistributedEventBus distributedEventBus,
//            ILogger<DeadLetterQueueAppService> logger)
//        {
//            _messageRepository = messageRepository;
//            _distributedEventBus = distributedEventBus;
//            _logger = logger;
//        }

//        public async Task<PagedResultDto<DeadLetterMessageDto>> GetDeadLetterMessagesAsync(PagedAndSortedResultRequestDto input)
//        {
//            var queryable = await _messageRepository.GetQueryableAsync();
            
//            queryable = queryable
//                .Where(m => m.Status == MessageStatus.Failed)
//                .OrderByDescending(m => m.CreationTime)
//                .Skip(input.SkipCount)
//                .Take(input.MaxResultCount);

//            var messages = await AsyncExecuter.ToListAsync(queryable);
//            var totalCount = await AsyncExecuter.CountAsync(
//                await _messageRepository.GetQueryableAsync()
//                    .Where(m => m.Status == MessageStatus.Failed));

//            var dtos = ObjectMapper.Map<List<Message>, List<DeadLetterMessageDto>>(messages);

//            return new PagedResultDto<DeadLetterMessageDto>(totalCount, dtos);
//        }

//        public async Task RequeueMessageAsync(Guid messageId)
//        {
//            var message = await _messageRepository.GetAsync(messageId);
            
//            if (message.Status != MessageStatus.Failed)
//            {
//                throw new UserFriendlyException($"Message {messageId} is not in failed status. Current status: {message.Status}");
//            }

//            // Reset retry count and schedule for immediate retry
//            message.MarkAsRetrying();
//            message.ScheduleNextRetry(TimeSpan.Zero);
            
//            await _messageRepository.UpdateAsync(message, autoSave: true);

//            // Republish to event bus
//            var sendMsgEto = ObjectMapper.Map<Message, SendOneWayMessageEto>(message);
//            sendMsgEto.From = "System";
//            sendMsgEto.IdempotencyKey = message.IdempotencyKey;
//            sendMsgEto.Priority = message.Priority;

//            await _distributedEventBus.PublishAsync(sendMsgEto);

//            _logger.LogInformation("Requeued message {MessageId} from dead letter queue", messageId);
//        }

//        public async Task RequeueMultipleMessagesAsync(List<Guid> messageIds)
//        {
//            foreach (var messageId in messageIds)
//            {
//                try
//                {
//                    await RequeueMessageAsync(messageId);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Error requeuing message {MessageId}", messageId);
//                }
//            }
//        }

//        public async Task DeleteMessageAsync(Guid messageId)
//        {
//            var message = await _messageRepository.GetAsync(messageId);
            
//            if (message.Status != MessageStatus.Failed)
//            {
//                throw new UserFriendlyException($"Only failed messages can be deleted from DLQ. Current status: {message.Status}");
//            }

//            await _messageRepository.DeleteAsync(message);
//            _logger.LogInformation("Deleted message {MessageId} from dead letter queue", messageId);
//        }
//    }
//}

