using System;
using System.Threading.Tasks;

namespace Esh3arTech.Messages
{
    public interface IMessageStatusUpdater
    {
        Task UpdateStatusAsync(Guid messageId, MessageStatus status);
    }
}
