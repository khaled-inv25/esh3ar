using Esh3arTech.Messages;
using Esh3arTech.Web.Hubs;
using Esh3arTech.Web.MobileUsers;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace Esh3arTech.Web.EventBus.DistributedHandlers
{
    public class DistributedSendMessageHandler : IDistributedEventHandler<SendMessageEto>, ITransientDependency
    {
        private readonly IHubContext<OnlineMobileUserHub> _hubContext;
        private readonly OnlineUserTrackerService _onlineUserTrackerService;

        public DistributedSendMessageHandler(
            IHubContext<OnlineMobileUserHub> hubContext, 
            OnlineUserTrackerService onlineUserTrackerService)
        {
            _hubContext = hubContext;
            _onlineUserTrackerService = onlineUserTrackerService;
        }

        public async Task HandleEventAsync(SendMessageEto eventData)
        {
            await SendRealTimeMessageToClient(eventData.Id, eventData.RecipientPhoneNumber, eventData.MessageContent, eventData.From!);
        }

        private async Task SendRealTimeMessageToClient(Guid id, string phoneNumber, string messageContent, string from)
        {
            var connectionId = _onlineUserTrackerService.GetFirstConnectionId(phoneNumber);

            if (!string.IsNullOrEmpty(connectionId))
            {
                var model = new SendMessageModel
                {
                    Id = id,
                    RecipientPhoneNumber = phoneNumber,
                    MessageContent = messageContent,
                    From = from
                };

                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", JsonSerializer.Serialize(model));
            }
        }
    }

    public class SendMessageModel
    {
        public Guid Id { get; set; }
        public string RecipientPhoneNumber { get; set; }
        public string MessageContent { get; set; }
        public string From { get; set; }

    }
}
