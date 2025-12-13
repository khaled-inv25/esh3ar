using System.Threading.Tasks;
using Volo.Abp.EventBus.Distributed;

namespace Esh3arTech.Messages
{
    public class MessageAppService : Esh3arTechAppService, IMessageAppService
    {
        private readonly IDistributedEventBus _distributedEventBus;

        public MessageAppService(
            IDistributedEventBus distributedEventBus)
        {
            _distributedEventBus = distributedEventBus;
        }

        public async Task ReceiveMessage(MessagePayloadDto input)
        {
            await _distributedEventBus.PublishAsync(ObjectMapper.Map<MessagePayloadDto, SendMessageEto>(input));
        }
    }
}
