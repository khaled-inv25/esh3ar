using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.Messages
{
    public interface IMessageAppService
    {
        Task<MessageDto> SendOneWayMessageAsync(SendOneWayMessageDto input);

        Task<MessageDto> SendMessageFromUiAsync(SendOneWayMessageWithAttachmentFromUiDto input);

        Task<IReadOnlyList<PendingMessageDto>> GetPendingMessagesAsync(string phoneNumber);

        Task<PagedResultDto<MessageInListDto>> GetOneWayMessagesAsync(PagedAndSortedResultRequestDto input);

        Task<object> GetMessageById(Guid messageId);

        Task UpdateMessage(object msg);
    }
}
