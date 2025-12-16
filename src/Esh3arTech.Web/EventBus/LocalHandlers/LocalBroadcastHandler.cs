using Esh3arTech.Messages;
using Esh3arTech.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace Esh3arTech.Web.EventBus.LocalHandlers
{
    public class LocalBroadcastHandler : ILocalEventHandler<BroadcastMessageEvent>, ITransientDependency
    {
        private readonly IHubContext<OnlineMobileUserHub> _hubContext;

        public LocalBroadcastHandler(IHubContext<OnlineMobileUserHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task HandleEventAsync(BroadcastMessageEvent eventData)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveBroadcastMessage", JsonSerializer.Serialize(eventData));
        }
    }
}
