using Esh3arTech.MobileUsers;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace Esh3arTech.Messages.Handlers
{
    public class LocalMessageHandler : ILocalEventHandler<SendMessageEvent>, ITransientDependency
    {
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly IOnlineMobileUserManager _onlineMobileUserManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public LocalMessageHandler(
            IDistributedEventBus distributedEventBus,
            IOnlineMobileUserManager onlineMobileUserManager,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _distributedEventBus = distributedEventBus;
            _onlineMobileUserManager = onlineMobileUserManager;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task HandleEventAsync(SendMessageEvent eventData)
        {
            if(_onlineMobileUserManager.IsConnected(eventData.PhoneNumber))
            {
                var eto = new SendMessageEto()
                {
                    PhoneNumber = eventData.PhoneNumber,
                    MessageContent = eventData.MessageContent
                };

                //using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
                await _distributedEventBus.PublishAsync(eto);
                //await uow.CompleteAsync();
            }
            else
            {
                // save to db with pending status for later processing.
            }
        }
    }
}
