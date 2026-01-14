using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;

namespace Esh3arTech.Messages
{
    public interface IMessageAppService
    {
        Task<bool> IngestionBatchMessageAsync(SendBatchMessageDto input);

        Task<MessageDto> IngestionSendOneWayMessageAsync(SendOneWayMessageDto input);

        Task<MessageDto> SendMessageFromUiAsync(SendOneWayMessageDto input);

        Task<MessageDto> SendMessageFromUiWithAttachmentAsync(SendOneWayMessageWithAttachmentFromUiDto input);

        Task SendMessagesFromFile(IRemoteStreamContent file);

        Task<IReadOnlyList<PendingMessageDto>> GetPendingMessagesAsync(string phoneNumber);

        Task<PagedResultDto<MessageInListDto>> GetOneWayMessagesAsync(PagedAndSortedResultRequestDto input);

        Task<object> GetMessageById(Guid messageId);

        Task<bool> IsMessageDeliveredOrSentAsync(Guid messageId);

        Task UpdateMessage(object msg);
    }
}
