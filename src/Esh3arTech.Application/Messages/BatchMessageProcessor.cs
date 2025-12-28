using Esh3arTech.Messages.Eto;
using Esh3arTech.Messages.Idempotency;
using Esh3arTech.Messages.RetryPolicy;
using Esh3arTech.Messages.SendBehavior;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;

namespace Esh3arTech.Messages
{
    public interface IBatchMessageProcessor : IApplicationService
    {
        Task<BatchMessageResultDto> SendBatchAsync(List<SendOneWayMessageDto> messages);
    }

    public class BatchMessageProcessor : ApplicationService, IBatchMessageProcessor
    {
        private readonly IMessageFactory _messageFactory;
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly IRepository<Message, Guid> _messageRepository;
        private readonly IIdempotencyService _idempotencyService;
        private readonly MessageReliabilityOptions _options;
        private readonly ILogger<BatchMessageProcessor> _logger;

        public BatchMessageProcessor(
            IMessageFactory messageFactory,
            IDistributedEventBus distributedEventBus,
            IRepository<Message, Guid> messageRepository,
            IIdempotencyService idempotencyService,
            IOptions<MessageReliabilityOptions> options,
            ILogger<BatchMessageProcessor> logger)
        {
            _messageFactory = messageFactory;
            _distributedEventBus = distributedEventBus;
            _messageRepository = messageRepository;
            _idempotencyService = idempotencyService;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<BatchMessageResultDto> SendBatchAsync(List<SendOneWayMessageDto> messages)
        {
            if (messages == null || messages.Count == 0)
            {
                throw new UserFriendlyException("Messages list cannot be empty");
            }

            if (messages.Count > _options.BatchSizeLimit)
            {
                throw new UserFriendlyException($"Batch size cannot exceed {_options.BatchSizeLimit} messages");
            }

            var results = new List<MessageResultDto>();
            var successCount = 0;
            var failureCount = 0;

            // Validate all messages first
            foreach (var input in messages)
            {
                try
                {
                    // Validate message
                    if (string.IsNullOrWhiteSpace(input.RecipientPhoneNumber))
                    {
                        results.Add(new MessageResultDto
                        {
                            Success = false,
                            ErrorMessage = "Recipient phone number is required"
                        });
                        failureCount++;
                        continue;
                    }

                    // Create message
                    var messageManager = _messageFactory.Create(MessageType.OneWay);
                    var createdMessage = await messageManager.CreateMessageAsync(input.RecipientPhoneNumber, input.MessageContent);

                    // Generate and set idempotency key
                    var idempotencyKey = await _idempotencyService.GenerateKeyAsync(createdMessage.Id);
                    createdMessage.SetIdempotencyKey(idempotencyKey);

                    createdMessage.SetMessageStatusType(MessageStatus.Queued);
                    await _messageRepository.InsertAsync(createdMessage, autoSave: true);

                    // Publish event
                    var sendMsgEto = ObjectMapper.Map<Message, SendOneWayMessageEto>(createdMessage);
                    sendMsgEto.From = CurrentUser?.Name ?? "System";
                    sendMsgEto.IdempotencyKey = idempotencyKey;
                    sendMsgEto.Priority = createdMessage.Priority;

                    await _distributedEventBus.PublishAsync(sendMsgEto);

                    results.Add(new MessageResultDto
                    {
                        MessageId = createdMessage.Id,
                        Success = true
                    });
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing batch message to {Recipient}", input.RecipientPhoneNumber);
                    results.Add(new MessageResultDto
                    {
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                    failureCount++;
                }
            }

            return new BatchMessageResultDto
            {
                TotalMessages = messages.Count,
                SuccessCount = successCount,
                FailureCount = failureCount,
                Results = results
            };
        }
    }

    public class BatchMessageResultDto
    {
        public int TotalMessages { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<MessageResultDto> Results { get; set; } = new();
    }

    public class MessageResultDto
    {
        public Guid? MessageId { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

