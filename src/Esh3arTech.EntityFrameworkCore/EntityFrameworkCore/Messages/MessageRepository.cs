using Esh3arTech.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Esh3arTech.EntityFrameworkCore.Messages
{
    public class MessageRepository : EfCoreRepository<Esh3arTechDbContext, Message, Guid>, IMessageRepository
    {
        public MessageRepository(IDbContextProvider<Esh3arTechDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<List<Message>> GetFailedOrQueuedMessagesAsync()
        {
            var messageQueryable = await GetQueryableAsync();

            var query = (from message in messageQueryable
                         where (message.Status == MessageStatus.Failed || message.Status == MessageStatus.Queued) &&
                         (message.NextRetryAt != null && message.NextRetryAt <= DateTime.UtcNow)
                         select message).Take(100);

            return await AsyncExecuter.ToListAsync(query);
        }
    }
}
