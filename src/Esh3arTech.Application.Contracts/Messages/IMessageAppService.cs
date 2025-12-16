using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esh3arTech.Messages
{
    public interface IMessageAppService
    {
        Task ReceiveMessage(MessagePayloadDto input);

        Task BroadcastAsync(BroadcastMessageDto input);

        Task<IReadOnlyList<PendingMessageDto>> GetPendingMessagesAsync(string phoneNumber);

        Task UpdateMessageStatusToDeliveredAsync(Guid messageId);
    }
}
