using Esh3arTech.Abp.Blob.Services;
using Esh3arTech.Messages.Specs;
using Esh3arTech.MobileUsers;
using Esh3arTech.MobileUsers.Specs;
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

namespace Esh3arTech.Messages
{
    public class MessageAppService : Esh3arTechAppService, IMessageAppService
    {
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly IRepository<Message, Guid> _messageRepository;
        private readonly IRepository<MobileUser, Guid> _mobileUserRepository;
        private readonly MessageManager _messageManager;
        private readonly IBlobService _blobService;

        public MessageAppService(
            IDistributedEventBus distributedEventBus,
            IRepository<Message, Guid> messageRepository,
            IRepository<MobileUser, Guid> mobileUserRepository,
            MessageManager messageManager,
            IBlobService blobService)
        {
            _distributedEventBus = distributedEventBus;
            _messageRepository = messageRepository;
            _mobileUserRepository = mobileUserRepository;
            _messageManager = messageManager;
            _blobService = blobService;
        }

        public async Task<IReadOnlyList<PendingMessageDto>> GetPendingMessagesAsync(string phoneNumber)
        {
            phoneNumber = MobileNumberPreparator.PrepareMobileNumber(phoneNumber);

            if (!await _mobileUserRepository.AnyAsync(new MobileVerifiedSpecification(phoneNumber).ToExpression()))
            {
                throw new UserFriendlyException("Mobile not found or not verified!");
            }

            var pendingMessages = await _messageRepository.GetListAsync(new PendingMessageSpecification(phoneNumber).ToExpression());

            return ObjectMapper.Map<List<Message>, List<PendingMessageDto>>(pendingMessages);
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

            if (!await _mobileUserRepository.AnyAsync(new MobileVerifiedSpecification(input.RecipientPhoneNumber).ToExpression()))
            {
                throw new UserFriendlyException("Mobile not found or not verified!");
            }

            var currentUserId = CurrentUser.Id!.Value;
            var createdMessage = await _messageManager.CreateOneWayMessage(currentUserId, input.RecipientPhoneNumber);
            createdMessage.SetSubject(input.Subject);
            createdMessage.SetMessageStatusType(MessageStatus.Pending);
            createdMessage.SetMessageContentOrNull(input.MessageContent);

            var sendMsgEto = ObjectMapper.Map<Message, SendOneWayMessageEto>(createdMessage);
            sendMsgEto.From = CurrentUser.Name!;

            await _distributedEventBus.PublishAsync(sendMsgEto);

            createdMessage.SetMessageStatusType(MessageStatus.Queued);
            await _messageRepository.InsertAsync(createdMessage);

            return new MessageDto { Id = createdMessage.Id };
        }

        //[Authorize(Esh3arTechPermissions.Esh3arSendMessages)]
        public async Task<MessageDto> SendMessageWithAttachmentAsync(SendOneWayMessageWithAttachmentDto input)
        {
            input.RecipientPhoneNumber = MobileNumberPreparator.PrepareMobileNumber(input.RecipientPhoneNumber);

            if (!await _mobileUserRepository.AnyAsync(new MobileVerifiedSpecification(input.RecipientPhoneNumber).ToExpression()))
            {
                throw new UserFriendlyException("Mobile not found or not verified!");
            }

            var currentUserId = CurrentUser.Id!.Value;
            var createdMessage = await _messageManager.CreateOneWayMessageWithAttachmentAsync(
                currentUserId, 
                input.RecipientPhoneNumber, 
                input.MessageContent, 
                input.Base64OrJson,
                input.Type
                );
            createdMessage.SetSubject(input.Subject);
            createdMessage.SetMessageStatusType(MessageStatus.Pending);

            var sendMsgEto = ObjectMapper.Map<Message, SendOneWayMessageEto>(createdMessage);
            sendMsgEto.From = CurrentUser.Name!;

            await _distributedEventBus.PublishAsync(sendMsgEto);

            return new MessageDto { Id = createdMessage.Id };
        }

        public async Task UpdateMessageStatus(UpdateMessageStatusDto input)
        {
            var message = await _messageRepository.GetAsync(input.Id);
            message.SetMessageStatusType(input.Status);
            await _messageRepository.UpdateAsync(message);
        }

        public async Task<MessageDto> SendMessageWithAttachmentFromUiAsync(SendOneWayMessageWithAttachmentFromUiDto input)
        {
            input.RecipientPhoneNumber = MobileNumberPreparator.PrepareMobileNumber(input.RecipientPhoneNumber);

            if (!await _mobileUserRepository.AnyAsync(new MobileVerifiedSpecification(input.RecipientPhoneNumber).ToExpression()))
            {
                throw new UserFriendlyException("Mobile not found or not verified!");
            }

            await _blobService.SaveToFileSystemAsync(input.ImageStreamContent, "blobName");

            return null;
        }
    }
}
