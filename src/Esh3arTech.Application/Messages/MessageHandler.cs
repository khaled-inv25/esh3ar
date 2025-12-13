using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace Esh3arTech.Messages
{
    public class MessageHandler : IDistributedEventHandler<SendMessageEto>, ITransientDependency
    {
        public async Task HandleEventAsync(SendMessageEto eventData)
        {
            await Task.CompletedTask;
        }
    }
}
