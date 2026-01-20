using Esh3arTech.Abp.Blob.Services;
using Esh3arTech.BackgroundJobs;
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
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;

namespace Esh3arTech.Messages
{
    public class MessageAppService : Esh3arTechAppService, IMessageAppService
    {
        #region Fields

        private readonly IMessageFactory _messageFactory;
        private readonly IMessageRepository _messageRepository;
        private readonly IMobileUserRepository _mobileUserRepository;
        private readonly IBlobService _blobService;
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly IBackgroundJobManager _backgroundJobManager;

        #endregion

        #region Ctor

        public MessageAppService(
            IMessageFactory messageFactory,
            IMessageRepository messageRepository,
            IMobileUserRepository mobileUserRepository,
            IBlobService blobService,
            IDistributedEventBus distributedEventBus,
            IBackgroundJobManager backgroundJobManager)
        {
            _messageFactory = messageFactory;
            _messageRepository = messageRepository;
            _mobileUserRepository = mobileUserRepository;
            _blobService = blobService;
            _distributedEventBus = distributedEventBus;
            _backgroundJobManager = backgroundJobManager;
        }

        #endregion

        #region Methods

        [Authorize(Esh3arTechPermissions.Esh3arSendMessages)]
        public async Task<bool> IngestionBatchMessageAsync(SendBatchMessageDto input)
        {
            var batchMessages = ObjectMapper.Map<List<BatchMessageItem>, List<BatchMessage>>(input.BatchMessages);
            var numbers = ObjectMapper.Map<List<BatchMessageItem>, List<EtTempMobileUserData>>(input.BatchMessages);

            var createdMessages = await CreateBatchMessageAsync(batchMessages, numbers);

            var arg = new BatchMessageIngestionArg
            {
                EnqueueMessages = ObjectMapper.Map<List<Message>, List<EnqueueBatchMessageDto>>(createdMessages),
            };

            await _backgroundJobManager.EnqueueAsync(arg);

            return true;
        }

        [Authorize(Esh3arTechPermissions.Esh3arSendMessages)]
        public async Task<MessageDto> IngestionSendOneWayMessageAsync(SendOneWayMessageDto input)
        {
            var createdMessage = await CreateMessageAsync(input);

            await _distributedEventBus.PublishAsync(ObjectMapper.Map<Message, SendOneWayMessageEto>(createdMessage));

            return new MessageDto { Id = createdMessage.Id };
        }

        [Authorize(Esh3arTechPermissions.Esh3arSendMessages)]
        public async Task<MessageDto> SendMessageFromUiAsync(SendOneWayMessageDto input)
        {
            var createdMessage = await CreateMessageAsync(input);

            await _backgroundJobManager.EnqueueAsync(new SendMessageFromUiArg { Message = createdMessage });

            return new MessageDto { Id = createdMessage.Id };
        }

        [Authorize(Esh3arTechPermissions.Esh3arSendMessages)]
        public async Task<MessageDto> SendMessageFromUiWithAttachmentAsync(SendOneWayMessageWithAttachmentFromUiDto input)
        {
            var messageManager = _messageFactory.Create(MessageType.OneWay);
            var createdMessageWithAttachment = await messageManager.CreateMessageWithAttachmentFromUiAsync(input.RecipientPhoneNumber, input.MessageContent, input.ImageStreamContent);

            var attachment = createdMessageWithAttachment.Attachments.FirstOrDefault();
            await _blobService.SaveToFileSystemAsync(input.ImageStreamContent, attachment!.FileName);

            var arg = ObjectMapper.Map<Message, SendMessageFromUiWithAttachmentArg>(createdMessageWithAttachment);
            await _backgroundJobManager.EnqueueAsync(arg);

            return new MessageDto { Id = createdMessageWithAttachment.Id };
        }

        public async Task SendMessagesFromFile(IRemoteStreamContent file)
        {
            var messageManager = _messageFactory.Create(MessageType.OneWay);
            var createdMessages = await messageManager.CreateMessagesFromFileAsync(file);

            var arg = new BatchMessageIngestionArg
            {
                EnqueueMessages = ObjectMapper.Map<List<Message>, List<EnqueueBatchMessageDto>>(createdMessages),
            };

            await _backgroundJobManager.EnqueueAsync(arg);
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

            var sorting = input.Sorting.IsNullOrWhiteSpace() ? $"{nameof(Message.CreationTime)} DESC" : input.Sorting;

            var filteredQueryByUser = queryable.Where(m => m.CreatorId == currentUserId);

            var filteredQuery = filteredQueryByUser
                .OrderByDescending(m => m.CreationTime)
                .PageBy(input.SkipCount, input.MaxResultCount);

            var messages = await AsyncExecuter.ToListAsync(filteredQuery);
            var totalCount = await AsyncExecuter.CountAsync(filteredQueryByUser);

            return new PagedResultDto<MessageInListDto>(totalCount, ObjectMapper.Map<List<Message>, List<MessageInListDto>>(messages));
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

        private async Task<List<Message>> CreateBatchMessageAsync(List<BatchMessage> data, List<EtTempMobileUserData> numbers)
        {
            var messageManager = _messageFactory.Create(MessageType.OneWay);
            var createdList = await messageManager.CreateBatchMessageAsync(data, numbers);

            return createdList;
        }

        #endregion
    }
}
