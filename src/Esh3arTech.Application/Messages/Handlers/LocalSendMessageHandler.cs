using Esh3arTech.MobileUsers;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;

namespace Esh3arTech.Messages.Handlers
{
    public class LocalSendMessageHandler : ILocalEventHandler<SendMessageEvent>, ITransientDependency
    {
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly IOnlineMobileUserManager _onlineMobileUserManager;
        private readonly IRepository<Message, Guid> _messageRepository;
        private readonly IGuidGenerator _guidGenerator;

        public LocalSendMessageHandler(
            IDistributedEventBus distributedEventBus,
            IOnlineMobileUserManager onlineMobileUserManager,
            IRepository<Message, Guid> messageRepository,
            IGuidGenerator guidGenerator)
        {
            _distributedEventBus = distributedEventBus;
            _onlineMobileUserManager = onlineMobileUserManager;
            _messageRepository = messageRepository;
            _guidGenerator = guidGenerator;
        }

        public async Task HandleEventAsync(SendMessageEvent eventData)
         {
            if (_onlineMobileUserManager.IsConnected(eventData.RecipientPhoneNumber))
            {
                var id = _guidGenerator.Create();
                
                var eto = new SendMessageEto()
                {
                    Id = id,
                    RecipientPhoneNumber = eventData.RecipientPhoneNumber,
                    MessageContent = eventData.MessageContent,
                    From = eventData.From
                };

                await _distributedEventBus.PublishAsync(eto);

                var msg = new Message(id, eventData.RecipientPhoneNumber, "Subject");
                msg.SetContentType(MessageContentType.Text);
                msg.SetMessageStatusType(MessageStatus.Sent);
                msg.SetMessageContent(eventData.MessageContent);

                await _messageRepository.InsertAsync(msg);
            }
            else
            {
                var msg = new Message(_guidGenerator.Create(), eventData.RecipientPhoneNumber, "Subject");
                msg.SetContentType(MessageContentType.Text);
                msg.SetMessageStatusType(MessageStatus.Pending);
                msg.SetMessageContent(eventData.MessageContent);
                await _messageRepository.InsertAsync(msg);
            }
        }
    }
}
