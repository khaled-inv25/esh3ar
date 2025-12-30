using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace Esh3arTech.Messages
{
    public class MessageStatusUpdater : IMessageStatusUpdater, ITransientDependency
    {
        private readonly IRepository<Message, Guid> _messageRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public MessageStatusUpdater(
            IRepository<Message, Guid> messageRepository, 
            IUnitOfWorkManager unitOfWorkManager)
        {
            _messageRepository = messageRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task SetMessageStatusToDeliveredInNewTransactionAsync(Guid messageId)
        {
            await SetMessageStatus(messageId, MessageStatus.Delivered);
        }

        public async Task SetMessageStatusToPendingInNewTransactionAsync(Guid messageId)
        {
            await SetMessageStatus(messageId, MessageStatus.Pending);
        }

        public async Task SetMessageStatusToSentInNewTransactionAsync(Guid messageId)
        {
            await SetMessageStatus(messageId, MessageStatus.Sent);
        }

        private async Task SetMessageStatus(Guid messageId, MessageStatus status)
        {
            using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);

            var message = await _messageRepository.GetAsync(messageId);

            if (message.Status.Equals(status))
            {
                return;
            }

            message.SetMessageStatusType(status);
            await _messageRepository.UpdateAsync(message);
            await uow.CompleteAsync();
        }
    }
}
