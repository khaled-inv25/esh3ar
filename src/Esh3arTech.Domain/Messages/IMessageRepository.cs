using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Esh3arTech.Messages
{
    public interface IMessageRepository : IRepository<Message, Guid>
    {
        Task<List<Message>> GetFailedOrQueuedMessagesAsync();

        Task BulkInsertMessages(List<Message> messages);
    }
}
