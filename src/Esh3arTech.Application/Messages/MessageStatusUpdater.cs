using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace Esh3arTech.Messages
{
    public class MessageStatusUpdater : IMessageStatusUpdater, ITransientDependency
    {
        private readonly IRepository<Message, Guid> _messageRepository;
        private readonly ILogger<MessageStatusUpdater> _logger;

        public MessageStatusUpdater(
            IRepository<Message, Guid> messageRepository,
            ILogger<MessageStatusUpdater> logger)
        {
            _messageRepository = messageRepository;
            _logger = logger;
        }

        public async Task UpdateStatusAsync(Guid messageId, MessageStatus status)
        {
            var message = await _messageRepository.FindAsync(messageId);
            if (message == null)
            {
                _logger.LogWarning($"UpdateStatusAsync: message {messageId} not found (possibly deleted). Skipping update to {status}.");
                return;
            }

            _logger.LogInformation($"Updating message {messageId} status from {message.Status} to {status}.");

            try
            {
                message.SetMessageStatusType(status);
                await _messageRepository.UpdateAsync(message);
                _logger.LogInformation($"Message {messageId} status updated to {status}.");
            }
            catch (AbpDbConcurrencyException ex)
            {
                _logger.LogError(ex, $"Concurrency exception updating message {messageId} status.");
                throw new UserFriendlyException($"UpdateStatusAsync {ex.Message}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating message {messageId} status.");
                throw;
            }
        }
    }
}
