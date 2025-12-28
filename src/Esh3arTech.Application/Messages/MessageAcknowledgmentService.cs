using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esh3arTech.Messages.RetryPolicy;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Esh3arTech.Messages
{
    public interface IMessageAcknowledgmentService : IApplicationService
    {
        Task AcknowledgeDeliveryAsync(Guid messageId);
        Task AcknowledgeBatchAsync(List<Guid> messageIds);
    }

    public class MessageAcknowledgmentService : ApplicationService, IMessageAcknowledgmentService
    {
        private readonly IRepository<Message, Guid> _messageRepository;
        private readonly IMessageStatusUpdater _messageStatusUpdater;
        private readonly MessageReliabilityOptions _options;
        private readonly ILogger<MessageAcknowledgmentService> _logger;

        public MessageAcknowledgmentService(
            IRepository<Message, Guid> messageRepository,
            IMessageStatusUpdater messageStatusUpdater,
            IOptions<MessageReliabilityOptions> options,
            ILogger<MessageAcknowledgmentService> logger)
        {
            _messageRepository = messageRepository;
            _messageStatusUpdater = messageStatusUpdater;
            _options = options.Value;
            _logger = logger;
        }

        public async Task AcknowledgeDeliveryAsync(Guid messageId)
        {
            var message = await _messageRepository.GetAsync(messageId);
            
            if (message.Status != MessageStatus.Sent && message.Status != MessageStatus.Pending)
            {
                throw new UserFriendlyException($"Message {messageId} cannot be acknowledged. Current status: {message.Status}");
            }

            await _messageStatusUpdater.UpdateStatusAsync(messageId, MessageStatus.Delivered);
            _logger.LogInformation("Message {MessageId} acknowledged as delivered", messageId);
        }

        public async Task AcknowledgeBatchAsync(List<Guid> messageIds)
        {
            foreach (var messageId in messageIds)
            {
                try
                {
                    await AcknowledgeDeliveryAsync(messageId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error acknowledging message {MessageId}", messageId);
                }
            }
        }
    }
}

