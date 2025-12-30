using System.Threading.Tasks;
using System;

namespace Esh3arTech.Messages
{
    public interface IMessageStatusUpdater
    {
        Task SetMessageStatusToSentInNewTransactionAsync(Guid messageId);

        Task SetMessageStatusToPendingInNewTransactionAsync(Guid messageId);

        Task SetMessageStatusToDeliveredInNewTransactionAsync(Guid messageId);
    }
}
