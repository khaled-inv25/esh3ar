using Esh3arTech.Messages;
using Esh3arTech.Web.Hubs;
using Esh3arTech.Web.MobileUsers;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using static Esh3arTech.Esh3arTechConsts;

namespace Esh3arTech.Web.MessagesHandler
{
    public class MessagesDeliveryHandler : IDistributedEventHandler<SendOneWayMessageEto>, ITransientDependency
    {
        private readonly IHubContext<OnlineMobileUserHub> _hubContext;
        private readonly OnlineUserTrackerService _onlineUserTrackerService;
        private readonly IMessageAppService _messageAppService;

        public MessagesDeliveryHandler(
            IHubContext<OnlineMobileUserHub> hubContext,
            OnlineUserTrackerService onlineUserTrackerService,
            IMessageAppService messageAppService)
        {
            _hubContext = hubContext;
            _onlineUserTrackerService = onlineUserTrackerService;
            _messageAppService = messageAppService;
        }

        public async Task HandleEventAsync(SendOneWayMessageEto eventData)
        {
            await SendRealTimeOrPendMessageAsync(eventData.Id, eventData.RecipientPhoneNumber, eventData.MessageContent, eventData.From);
        }

        private async Task SendRealTimeOrPendMessageAsync(Guid id, string phoneNumber, string messageContent, string from)
        {
            var connectionId = await _onlineUserTrackerService.GetFirstConnectionIdByPhoneNumberAsync(phoneNumber);

            if (!string.IsNullOrEmpty(connectionId))
            {
                var model = new SendMessageModel
                {
                    Id = id,
                    RecipientPhoneNumber = phoneNumber,
                    MessageContent = messageContent,
                    From = from
                };

                await _hubContext.Clients.Client(connectionId).SendAsync(HubMethods.ReceiveMessage, JsonSerializer.Serialize(model));
            }
            else
            {
                await _messageAppService.UpdateMessageStatus(new UpdateMessageStatusDto() { Id = id, Status = MessageStatus.Pending });
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
}
