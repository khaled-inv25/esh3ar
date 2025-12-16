using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Uow;

namespace Esh3arTech.Messages
{
    public class MessageAppService : Esh3arTechAppService, IMessageAppService
    {
        private readonly ILocalEventBus _localEventBus;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<Message, Guid> _messageRepository;

        public MessageAppService(
            ILocalEventBus localEventBus,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<Message, Guid> messageRepository)
        {
            _localEventBus = localEventBus;
            _unitOfWorkManager = unitOfWorkManager;
            _messageRepository = messageRepository;
        }

        public async Task<IReadOnlyList<PendingMessageDto>> GetPendingMessagesAsync(string phoneNumber)
        {
            var pendingMessages = await _messageRepository.GetListAsync(m => m.RecipientPhoneNumber.Equals(phoneNumber) && m.Status.Equals(MessageStatus.Pending));

            IReadOnlyList<PendingMessageDto> list = ObjectMapper.Map<List<Message>, List<PendingMessageDto>>(pendingMessages);

            return list;
        }

        [Authorize(Roles = "Sender")]
        public async Task ReceiveMessage(MessagePayloadDto input)
        {

            using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
            await _localEventBus.PublishAsync(ObjectMapper.Map<MessagePayloadDto, SendMessageEvent>(input));
            await uow.CompleteAsync();
        }

        public async Task BroadcastAsync(BroadcastMessageDto input)
        {
            using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
            await _localEventBus.PublishAsync(ObjectMapper.Map<BroadcastMessageDto, BroadcastMessageEvent>(input));
            await uow.CompleteAsync();
        }

        public async Task UpdateMessageStatusToDeliveredAsync(Guid messageId)
        {
            var message = await _messageRepository.GetAsync(messageId);

            message.Status = MessageStatus.Delivered;
            await _messageRepository.UpdateAsync(message);
        }
    }
}
