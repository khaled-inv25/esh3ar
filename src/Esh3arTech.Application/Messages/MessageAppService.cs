using Esh3arTech.Abp.Blob.Services;
using Esh3arTech.Messages.Buffer;
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
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace Esh3arTech.Messages
{
    public class MessageAppService : Esh3arTechAppService, IMessageAppService
    {
        #region Fields

        private readonly IMessageFactory _messageFactory;
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly IMessageRepository _messageRepository;
        private readonly IRepository<MobileUser, Guid> _mobileUserRepository;
        private readonly IBlobService _blobService;
        private readonly IHighThroughputMessageBuffer _highThroughputMessageBuffer;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        #endregion

        #region Ctor

        public MessageAppService(
            IMessageFactory messageFactory,
            IDistributedEventBus distributedEventBus,
            IMessageRepository messageRepository,
            IRepository<MobileUser, Guid> mobileUserRepository,
            IBlobService blobService,
            IHighThroughputMessageBuffer highThroughputMessageBuffer,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _messageFactory = messageFactory;
            _distributedEventBus = distributedEventBus;
            _messageRepository = messageRepository;
            _mobileUserRepository = mobileUserRepository;
            _blobService = blobService;
            _highThroughputMessageBuffer = highThroughputMessageBuffer;
            _unitOfWorkManager = unitOfWorkManager;
        }

        #endregion

        #region Methods

        public async Task<bool> IngestionBatchMessageAsync(SendBatchMessageDto input)
        {
            var map = ObjectMapper.Map<List<SendOneWayMessageDto>, List<BatchMessage>>(input.BatchMessages);
            var createdMessages = await CreateBatchMessageAsync(map);

            return true;
        }

        [Authorize(Esh3arTechPermissions.Esh3arSendMessages)]
        public async Task<MessageDto> IngestionSendOneWayMessageAsync(SendOneWayMessageDto input)
        {
            var createdMessage = await CreateMessageAsync(input);

            await _highThroughputMessageBuffer.TryWriteAsync(createdMessage, TimeSpan.FromMilliseconds(50));

            return new MessageDto { Id = createdMessage.Id };
        }

        [Authorize(Esh3arTechPermissions.Esh3arSendMessages)]
        public async Task<MessageDto> SendMessageFromUiAsync(SendOneWayMessageDto input)
        {
            var createdMessage = await CreateMessageAsync(input);

            using var uow = _unitOfWorkManager.Begin(requiresNew: true);
            await _messageRepository.InsertAsync(createdMessage);
            await uow.CompleteAsync();

            return new MessageDto { Id = createdMessage.Id };
        }
        
        [Authorize(Esh3arTechPermissions.Esh3arSendMessages)]
        public async Task<MessageDto> SendMessageFromUiWithAttachmentAsync(SendOneWayMessageWithAttachmentFromUiDto input)
        {
            var messageManager = _messageFactory.Create(MessageType.OneWay);
            var createdMessageWithAttachment = await messageManager.CreateMessageWithAttachmentFromUiAsync(input.RecipientPhoneNumber, input.MessageContent, input.ImageStreamContent);

            var attachment = createdMessageWithAttachment.Attachments.FirstOrDefault();
            await _blobService.SaveToFileSystemAsync(input.ImageStreamContent, attachment!.FileName);

            using var uow = _unitOfWorkManager.Begin(requiresNew: true);
            await _messageRepository.InsertAsync(createdMessageWithAttachment);
            await uow.CompleteAsync();

            return new MessageDto { Id = createdMessageWithAttachment.Id };
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

        public async Task<object> GetMessageById(Guid messageId)
        {
            return await _messageRepository.GetAsync(messageId);
        }

        public async Task UpdateMessage(object msg)
        {
            await _messageRepository.UpdateAsync((Message)msg, autoSave: true);
        }

        public async Task<bool> IsMessageDeliveredOrSentAsync(Guid messageId)
        {
            return await _messageRepository.AnyAsync(m => m.Id == messageId && (m.Status == MessageStatus.Delivered || m.Status == MessageStatus.Sent));
        }

        private async Task<Message> CreateMessageAsync(SendOneWayMessageDto input)
        {
            var messageManager = _messageFactory.Create(MessageType.OneWay);
            var createdMessage = await messageManager.CreateMessageAsync(input.RecipientPhoneNumber, input.MessageContent);

            createdMessage.SetMessageStatusType(MessageStatus.Queued);

            return createdMessage;
        }

        private async Task<List<Message>> CreateBatchMessageAsync(List<BatchMessage> batchMessages)
        {
            var messageManager = _messageFactory.Create(MessageType.OneWay);
            var createdMessage = await messageManager.CreateBatchMessageAsync(batchMessages);

            return createdMessage;
        }

        #endregion
    }
}
