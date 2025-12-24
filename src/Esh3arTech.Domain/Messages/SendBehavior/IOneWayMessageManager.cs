using System;
using System.Threading.Tasks;
using Volo.Abp.Content;

namespace Esh3arTech.Messages.SendBehavior
{
    public interface IOneWayMessageManager
    {
        Task<Message> CreateMessageAsync(string recipient, string content);
        Task<Message> CreateMessageWithAttachmentAsync();
        Task<Message> CreateMessageWithAttachmentFromUiAsync(string recipient, string? content, IRemoteStreamContent stream);
    }
}
