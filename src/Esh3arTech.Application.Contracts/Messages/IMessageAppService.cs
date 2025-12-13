using System.Threading.Tasks;

namespace Esh3arTech.Messages
{
    public interface IMessageAppService
    {
        Task ReceiveMessage(MessagePayloadDto input);
    }
}
