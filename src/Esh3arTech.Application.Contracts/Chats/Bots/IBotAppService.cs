using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.Chats.Bots
{
    public interface IBotAppService
    {
        Task<PagedResultDto<UsersInListWithBotFeatureDto>> GetUsersWithBotFeatureAsync();
    }
}
