using Esh3arTech.MobileUsers;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;

namespace Esh3arTech.Messages
{
    public class MessageHandler : ILocalEventHandler<SendMessageEvent>, ITransientDependency
    {
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly IOnlineMobileUserManager _onlineMobileUserManager;

        public MessageHandler(
            IDistributedEventBus distributedEventBus, 
            IOnlineMobileUserManager onlineMobileUserManager)
        {
            _distributedEventBus = distributedEventBus;
            _onlineMobileUserManager = onlineMobileUserManager;
        }

        public async Task HandleEventAsync(SendMessageEvent eventData)
        {
            if(await _onlineMobileUserManager.IsConnectedAsync(eventData.PhoneNumber))
            {
                var eto = new SendMessageEto()
                {
                    PhoneNumber = eventData.PhoneNumber,
                    MessageContent = eventData.MessageContent
                };

                await _distributedEventBus.PublishAsync(eto);
            }
            else
            {
                // save to db with pending status for later processing.
            }
        }
    }
}
