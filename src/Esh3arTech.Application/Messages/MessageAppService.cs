using Esh3arTech.MobileUsers;
using Esh3arTech.Permissions;
using Esh3arTech.Plans;
using Esh3arTech.Utility;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;

namespace Esh3arTech.Messages
{
    public class MessageAppService : Esh3arTechAppService, IMessageAppService
    {
        private readonly ILocalEventBus _localEventBus;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<Message, Guid> _messageRepository;
        private readonly IRepository<MobileUser, Guid> _mobileUserRepository;
        private readonly MessageManager _messageManager;

        public MessageAppService(
            ILocalEventBus localEventBus,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<Message, Guid> messageRepository,
            IRepository<MobileUser, Guid> mobileUserRepository,
            MessageManager messageManager)
        {
            _localEventBus = localEventBus;
            _unitOfWorkManager = unitOfWorkManager;
            _messageRepository = messageRepository;
            _mobileUserRepository = mobileUserRepository;
            _messageManager = messageManager;
        }

        [Authorize]
        public async Task<IReadOnlyList<PendingMessageDto>> GetPendingMessagesAsync(string phoneNumber)
        {
            var pendingMessages = await _messageRepository.GetListAsync(m => m.RecipientPhoneNumber.Equals(phoneNumber) && m.Status.Equals(MessageStatus.Pending));

            IReadOnlyList<PendingMessageDto> list = ObjectMapper.Map<List<Message>, List<PendingMessageDto>>(pendingMessages);

            return list;
        }

        [Authorize(Esh3arTechPermissions.Esh3arSendMessages)]
        public async Task ReceiveMessageToRoutAsync(MessagePayloadDto input)
        {
            input.RecipientPhoneNumber = MobileNumberPreparator.PrepareMobileNumber(input.RecipientPhoneNumber);

            if (!await _mobileUserRepository.AnyAsync(m => m.MobileNumber.Equals(input.RecipientPhoneNumber) && m.Status.Equals(MobileUserRegisterStatus.Verified)))
            {
                throw new UserFriendlyException("Mobile not found or not verified!");
            }

            var currentUserId = CurrentUser.Id!.Value;
            var createdMessage = await _messageManager.CreateMessageAsync(
                currentUserId,
                input.RecipientPhoneNumber,
                "Subject",
                input.MessageContent
                );

            using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
            var @event = ObjectMapper.Map<Message, SendMessageEvent>(createdMessage);
            @event.From = CurrentUser.Name!;
            await _localEventBus.PublishAsync(@event);
            await uow.CompleteAsync();
        }

        public async Task BroadcastAsync(BroadcastMessageDto input)
        {
            using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
            await _localEventBus.PublishAsync(ObjectMapper.Map<BroadcastMessageDto, BroadcastMessageEvent>(input));
            await uow.CompleteAsync();
        }

        [Authorize]
        public async Task UpdateMessageStatusToDeliveredAsync(Guid messageId)
        {
            var message = await _messageRepository.GetAsync(messageId);

            message.SetMessageStatusType(MessageStatus.Delivered);
            await _messageRepository.UpdateAsync(message);
        }

        [Authorize]
        public async Task<PagedResultDto<MessageInListDto>> GetAllMessages()
        {
            var currentUserId = CurrentUser.Id!.Value;

            var messages = await _messageRepository.GetListAsync(m => m.CreatorId.Equals(currentUserId));
            var dtos = ObjectMapper.Map<List<Message>, List<MessageInListDto>>(messages);

            return new PagedResultDto<MessageInListDto>(dtos.Count, dtos);
        }
    }
}
