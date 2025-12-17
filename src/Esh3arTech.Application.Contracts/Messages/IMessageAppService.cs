using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.Messages
{
    public interface IMessageAppService
    {
        Task ReceiveMessageToRoutAsync(MessagePayloadDto input);

        Task BroadcastAsync(BroadcastMessageDto input);

        Task<IReadOnlyList<PendingMessageDto>> GetPendingMessagesAsync(string phoneNumber);

        Task UpdateMessageStatusToDeliveredAsync(Guid messageId);

        Task<PagedResultDto<MessageInListDto>> GetAllMessages();
    }
}
