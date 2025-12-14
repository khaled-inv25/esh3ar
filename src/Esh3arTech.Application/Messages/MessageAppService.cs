using System.Threading.Tasks;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Uow;

namespace Esh3arTech.Messages
{
    public class MessageAppService : Esh3arTechAppService, IMessageAppService
    {
        private readonly ILocalEventBus _localEventBus;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public MessageAppService(
            ILocalEventBus localEventBus, 
            IUnitOfWorkManager unitOfWorkManager)
        {
            _localEventBus = localEventBus;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task ReceiveMessage(MessagePayloadDto input)
        {
            using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
            await _localEventBus.PublishAsync(ObjectMapper.Map<MessagePayloadDto, SendMessageEvent>(input));
            await uow.CompleteAsync();
        }
    }
}
