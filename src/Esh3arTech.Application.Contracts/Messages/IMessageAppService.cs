using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.Messages
{
    public interface IMessageAppService
    {
        Task<IReadOnlyList<PendingMessageDto>> GetPendingMessagesAsync(string phoneNumber);

        Task<PagedResultDto<MessageInListDto>> GetOneWayMessagesAsync();

        Task<MessageDto> SendOneWayMessageAsync(SendOneWayMessageDto input);

        Task<MessageDto> SendOneWayMessageWithMediaAsync(SendOneWayMessageWithMediaDto input);

        Task UpdateMessageStatus(UpdateMessageStatusDto input);
    }
}
