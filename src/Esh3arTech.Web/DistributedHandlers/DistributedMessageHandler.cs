using Esh3arTech.Messages;
using Esh3arTech.Web.Hubs;
using Esh3arTech.Web.MobileUsers;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace Esh3arTech.Web.DistributedHandlers
{
    public class DistributedMessageHandler : IDistributedEventHandler<SendMessageEto>, ITransientDependency
    {
        private readonly IHubContext<OnlineMobileUserHub> _hubContext;
        private readonly OnlineUserTrackerService _onlineUserTrackerService;

        public DistributedMessageHandler(
            IHubContext<OnlineMobileUserHub> hubContext, 
            OnlineUserTrackerService onlineUserTrackerService)
        {
            _hubContext = hubContext;
            _onlineUserTrackerService = onlineUserTrackerService;
        }

        public async Task HandleEventAsync(SendMessageEto eventData)
        {
            await SendMessageToClient(eventData.PhoneNumber, eventData.MessageContent);
        }

        private async Task SendMessageToClient(string phoneNumber, string messageContent)
        {
            var connectionId = _onlineUserTrackerService.GetFirstConnectionId(phoneNumber);

            if (!string.IsNullOrEmpty(connectionId))
            {
                var model = new SendMessageModel
                {
                    PhoneNumber = phoneNumber,
                    MessageContent = messageContent
                };

                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", JsonSerializer.Serialize(model));            }
        }
    }

    public class SendMessageModel
    {
        public string PhoneNumber { get; set; }
        public string MessageContent { get; set; }
    }
}
