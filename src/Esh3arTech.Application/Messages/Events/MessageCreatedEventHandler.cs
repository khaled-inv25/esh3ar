using Esh3arTech.Messages.Delivery;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;

namespace Esh3arTech.Messages.Events
{
    public class MessageCreatedEventHandler : ILocalEventHandler<EntityCreatedEventData<Message>>, ITransientDependency
    {
        private readonly IMessageDeliveryService _messageDeliveryService;

        public MessageCreatedEventHandler(IMessageDeliveryService messageDeliveryService)
        {
            _messageDeliveryService = messageDeliveryService;
        }

        public async Task HandleEventAsync(EntityCreatedEventData<Message> eventData)
        {
            DeliverMessageDto deliverMessageDto;

            if (eventData.Entity.Attachments.Count != 0)
            {
                var attachment = eventData.Entity.Attachments.First();

                deliverMessageDto = new DeliverMessageDto()
                {
                    Id = eventData.Entity.Id,
                    RecipientPhoneNumber = eventData.Entity.RecipientPhoneNumber,
                    MessageContent = eventData.Entity.MessageContent,
                    CreatorId = eventData.Entity.CreatorId!.Value,
                    Status = eventData.Entity.Status,
                    AccessUrl = attachment.AccessUrl,
                    UrlExpiresAt = attachment.UrlExpiresAt
                };
            }
            else
            {
                deliverMessageDto = new DeliverMessageDto()
                {
                    Id = eventData.Entity.Id,
                    RecipientPhoneNumber = eventData.Entity.RecipientPhoneNumber,
                    MessageContent = eventData.Entity.MessageContent,
                    CreatorId = eventData.Entity.CreatorId!.Value,
                    Status = eventData.Entity.Status,
                    AccessUrl = null,
                    UrlExpiresAt = null
                };
            }

            await _messageDeliveryService.DeliverMessageAsync(deliverMessageDto);
        }
    }
}
