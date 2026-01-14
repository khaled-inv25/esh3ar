using Esh3arTech.MobileUsers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Content;

namespace Esh3arTech.Messages.SendBehavior
{
    public interface IOneWayMessageManager
    {
        Task<Message> CreateMessageAsync(string recipient, string content);
        Task<Message> CreateMessageWithAttachmentFromUiAsync(string recipient, string? content, IRemoteStreamContent stream);
        Task<List<Message>> CreateBatchMessageAsync(List<BatchMessage> data, List<EtTempMobileUserData> numbers);
        Task<List<Message>> CreateMessagesFromFileAsync(IRemoteStreamContent stream);
    }
}
