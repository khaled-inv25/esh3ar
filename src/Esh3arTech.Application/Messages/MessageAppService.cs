using Esh3arTech.Abp.Blob.Services;
using Esh3arTech.Messages.Eto;
using Esh3arTech.Messages.SendBehavior;
using Esh3arTech.Messages.Specs;
using Esh3arTech.MobileUsers;
using Esh3arTech.MobileUsers.Specs;
using Esh3arTech.Permissions;
using Esh3arTech.Utility;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IMessageFactory _messageFactory;
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly IMessageRepository _messageRepository;
        private readonly IRepository<MobileUser, Guid> _mobileUserRepository;
        private readonly IBlobService _blobService;
        private readonly IMessageStatusUpdater _messageStatusUpdater;

        public MessageAppService(
            IMessageFactory messageFactory,
            IDistributedEventBus distributedEventBus,
            IMessageRepository messageRepository,
            IRepository<MobileUser, Guid> mobileUserRepository,
            IBlobService blobService,
            IUnitOfWorkManager unitOfWorkManager,
            IMessageStatusUpdater messageStatusUpdater)
        {
            _messageFactory = messageFactory;
            _distributedEventBus = distributedEventBus;
            _messageRepository = messageRepository;
            _mobileUserRepository = mobileUserRepository;
            _blobService = blobService;
            _messageStatusUpdater = messageStatusUpdater;
        }

        [Authorize(Esh3arTechPermissions.Esh3arSendMessages)]
        public async Task<MessageDto> SendOneWayMessageAsync(SendOneWayMessageDto input)
        {
            var messageManager = _messageFactory.Create(MessageType.OneWay);
            var createdMessage = await messageManager.CreateMessageAsync(input.RecipientPhoneNumber, input.MessageContent);

            createdMessage.SetMessageStatusType(MessageStatus.Queued);
            await _messageRepository.InsertAsync(createdMessage, autoSave: true);

            var sendMsgEto = ObjectMapper.Map<Message, SendOneWayMessageEto>(createdMessage);
            sendMsgEto.From = CurrentUser.Name!;

            await _distributedEventBus.PublishAsync(sendMsgEto);

            return new MessageDto { Id = createdMessage.Id };
        }

        /*
        [Authorize(Esh3arTechPermissions.Esh3arSendMessages)]
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
        */
        
        [Authorize(Esh3arTechPermissions.Esh3arSendMessages)]
        public async Task<MessageDto> SendMessageWithAttachmentFromUiAsync(SendOneWayMessageWithAttachmentFromUiDto input)
        {
            var messageManager = _messageFactory.Create(MessageType.OneWay);
            var createdMessageWithAttachment = await messageManager.CreateMessageWithAttachmentFromUiAsync(input.RecipientPhoneNumber, input.MessageContent, input.ImageStreamContent);

            var attachment = createdMessageWithAttachment.Attachments.FirstOrDefault();
            await _blobService.SaveToFileSystemAsync(input.ImageStreamContent, attachment!.FileName);

            createdMessageWithAttachment.SetMessageStatusType(MessageStatus.Pending);

            var msgWithAttachmentEto = new SendOneWayMessageEto
            {
                Id = createdMessageWithAttachment.Id,
                RecipientPhoneNumber = createdMessageWithAttachment.RecipientPhoneNumber,
                MessageContent = input.MessageContent,
                From = CurrentUser.Name!,
                AccessUrl = attachment.AccessUrl
            };

            await _distributedEventBus.PublishAsync(msgWithAttachmentEto);

            createdMessageWithAttachment.SetMessageStatusType(MessageStatus.Queued);
            await _messageRepository.InsertAsync(createdMessageWithAttachment);

            return new MessageDto { Id = createdMessageWithAttachment.Id }; ;
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
        public async Task<PagedResultDto<MessageInListDto>> GetOneWayMessagesAsync(PagedAndSortedResultRequestDto input)
        {
            var queryable = await _messageRepository.GetQueryableAsync();
            var currentUserId = CurrentUser.Id!.Value;

            queryable = queryable
                .Where(m => m.CreatorId.Equals(currentUserId))
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .OrderByDescending(m => m.CreationTime);

            var messages = await AsyncExecuter.ToListAsync(queryable);
            var count = await AsyncExecuter.CountAsync(queryable);

            var dtos = ObjectMapper.Map<List<Message>, List<MessageInListDto>>(messages);

            return new PagedResultDto<MessageInListDto>(count, dtos);
        }

        public async Task UpdateMessageStatus(UpdateMessageStatusDto input)
        {
            await _messageStatusUpdater.UpdateStatusAsync(input.Id, input.Status);
        }

        public async Task<object> GetMessageById(Guid messageId)
        {
            return await _messageRepository.GetAsync(messageId);
        }

        public async Task UpdateMessage(object msg) => await _messageRepository.UpdateAsync((Message)msg, autoSave: true);
    }
}
