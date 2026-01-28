using System.Threading.Tasks;

namespace Esh3arTech.Chats
{
    public interface IChatService
    {
        Task<ReceiveToBusinessMessageDto> CreateMobileToBusinessMessageAsync(ReceiveToBusinessMessageDto input);

        Task<ReceiveToMobileMessageDto> CreateBusinessToMobileMessageAsync(ReceiveToMobileMessageDto input);
    }
}
