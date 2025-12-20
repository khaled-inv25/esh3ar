using Esh3arTech.MobileUsers;
using Esh3arTech.Permissions;
using Esh3arTech.Utility;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace Esh3arTech.Messages
{
    public class MessageAppService : Esh3arTechAppService, IMessageAppService
    {
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly IRepository<Message, Guid> _messageRepository;
        private readonly IRepository<MobileUser, Guid> _mobileUserRepository;
        private readonly MessageManager _messageManager;

        public MessageAppService(
            IDistributedEventBus distributedEventBus,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<Message, Guid> messageRepository,
            IRepository<MobileUser, Guid> mobileUserRepository,
            MessageManager messageManager)
        {
            _distributedEventBus = distributedEventBus;
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

        [Authorize]
        public async Task<PagedResultDto<MessageInListDto>> GetOneWayMessagesAsync()
        {
            var currentUserId = CurrentUser.Id!.Value;

            var messages = await _messageRepository.GetListAsync(m => m.CreatorId.Equals(currentUserId));
            var dtos = ObjectMapper.Map<List<Message>, List<MessageInListDto>>(messages);

            return new PagedResultDto<MessageInListDto>(dtos.Count, dtos);
        }

        [Authorize(Esh3arTechPermissions.Esh3arSendMessages)]
        public async Task<MessageDto> SendOneWayMessageAsync(SendOneWayMessageDto input)
        {
            input.RecipientPhoneNumber = MobileNumberPreparator.PrepareMobileNumber(input.RecipientPhoneNumber);

            if (!await _mobileUserRepository.AnyAsync(m => m.MobileNumber.Equals(input.RecipientPhoneNumber) && m.Status.Equals(MobileUserRegisterStatus.Verified)))
            {
                throw new UserFriendlyException("Mobile not found or not verified!");
            }

            var currentUserId = CurrentUser.Id!.Value;
            var createdMessage = await _messageManager.CreateOneWayMessage(currentUserId, input.RecipientPhoneNumber);
            createdMessage.SetSubject(input.Subject);
            createdMessage.SetMessageStatusType(MessageStatus.Pending);
            createdMessage.SetMessageContent(input.MessageContent);

            var sendMsgEto = ObjectMapper.Map<Message, SendOneWayMessageEto>(createdMessage);
            sendMsgEto.From = CurrentUser.Name!;

            await _distributedEventBus.PublishAsync(sendMsgEto);

            // to update the status to "Queued" immediately after enqueuing.
            createdMessage.SetMessageStatusType(MessageStatus.Queued);
            await _messageRepository.InsertAsync(createdMessage);

            return new MessageDto { Id = createdMessage.Id };
        }

        public async Task UpdateMessageStatus(UpdateMessageStatusDto input)
        {
            var message = await _messageRepository.GetAsync(input.Id);

            message.SetMessageStatusType(input.Status);
            await _messageRepository.UpdateAsync(message);
        }
    }
}
