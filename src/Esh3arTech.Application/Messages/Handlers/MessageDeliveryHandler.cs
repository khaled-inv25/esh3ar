using Esh3arTech.Messages.Delivery;
using Esh3arTech.Messages.Eto;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.ObjectMapping;

namespace Esh3arTech.Messages.Handlers
{
    public class MessageDeliveryHandler : IDistributedEventHandler<SendOneWayMessageEto>, ITransientDependency
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMessageDeliveryService _messageDeliveryService;
        private readonly IObjectMapper<SendOneWayMessageEto, Message> _objectMapper;

        public MessageDeliveryHandler(
            IMessageRepository messageRepository,
            IMessageDeliveryService messageDeliveryService,
            IObjectMapper<SendOneWayMessageEto, Message> objectMapper)
        {
            _messageRepository = messageRepository;
            _messageDeliveryService = messageDeliveryService;
            _objectMapper = objectMapper;
        }

        public async Task HandleEventAsync(SendOneWayMessageEto eventData)
        {
            var insertedMessage = await _messageRepository.InsertAsync(_objectMapper.Map(eventData));

            await _messageDeliveryService.DeliverMessageAsync(new DeliverMessageDto 
            {
                Id = insertedMessage.Id,
                RecipientPhoneNumber = insertedMessage.RecipientPhoneNumber,
                MessageContent = insertedMessage.MessageContent,
                CreatorId = insertedMessage.CreatorId!.Value,
                Status = insertedMessage.Status,
                AccessUrl = null ,
                UrlExpiresAt = null
            });
        }
    }
}
