using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Esh3arTech.Messages
{
    public interface IDeadLetterQueueAppService : IApplicationService
    {
        Task<PagedResultDto<DeadLetterMessageDto>> GetDeadLetterMessagesAsync(PagedAndSortedResultRequestDto input);
        Task RequeueMessageAsync(Guid messageId);
        Task RequeueMultipleMessagesAsync(List<Guid> messageIds);
        Task DeleteMessageAsync(Guid messageId);
    }
}

