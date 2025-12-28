using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.Messages
{
    public interface IMessageAppService
    {
        Task<MessageDto> SendOneWayMessageAsync(SendOneWayMessageDto input);

        //Task<MessageDto> SendMessageWithAttachmentAsync(SendOneWayMessageWithAttachmentDto input);

        Task<MessageDto> SendMessageWithAttachmentFromUiAsync(SendOneWayMessageWithAttachmentFromUiDto input);

        Task<IReadOnlyList<PendingMessageDto>> GetPendingMessagesAsync(string phoneNumber);

        Task<PagedResultDto<MessageInListDto>> GetOneWayMessagesAsync(PagedAndSortedResultRequestDto input);

        Task UpdateMessageStatus(UpdateMessageStatusDto input);

        Task<bool> IsExistsAsync(Guid messageId);

        Task<MessageStatus> GetMessageStatusById(Guid messageId);

        Task ResendOneWayMessageAsync(Guid messageId);
    }
}
