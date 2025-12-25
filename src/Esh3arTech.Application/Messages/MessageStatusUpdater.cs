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
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger<MessageStatusUpdater> _logger;

        public MessageStatusUpdater(
            IRepository<Message, Guid> messageRepository, 
            IUnitOfWorkManager unitOfWorkManager, 
            ILogger<MessageStatusUpdater> logger)
        {
            _messageRepository = messageRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
        }

        public async Task UpdateStatusAsync(Guid messageId, MessageStatus status)
        {
            using var uow = _unitOfWorkManager.Begin(requiresNew: true);

            var message = await _messageRepository.FindAsync(messageId);
            if (message == null)
            {
                _logger.LogWarning($"UpdateStatusAsync: message {messageId} not found (possibly deleted). Skipping update to {nameof(status)}.", messageId, status);
                return;
            }

            try
            {
                message.SetMessageStatusType(status);
                await uow.CompleteAsync();
            }
            catch (AbpDbConcurrencyException ex)
            {
                var exists = await _messageRepository.AnyAsync(m => m.Id == messageId);
                if (!exists)
                {
                    _logger.LogWarning(ex, $"UpdateStatusAsync: message {messageId} was deleted by another process while updating to {nameof(status)}. Treating as no-op.", messageId, status);
                    return;
                }

                _logger.LogError(ex, "UpdateStatusAsync: concurrency error while updating message {MessageId} to {Status}.", messageId, status);
                throw new UserFriendlyException("Message was modified by another process. Please retry the operation.");
            }
        }
    }
}
