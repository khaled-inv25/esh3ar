using System.Threading.Tasks;

namespace Esh3arTech.UserMessages
{
    public interface IUserMessagesAppService
    {
        Task<MessagesStatusDto> GetMessagesStatus();
    }
}
