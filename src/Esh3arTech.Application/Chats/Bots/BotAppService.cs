using Esh3arTech.Plans;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;

namespace Esh3arTech.Chats.Bots
{
    public class BotAppService : Esh3arTechAppService, IBotAppService
    {
        private readonly IUserPlanRepository _userPlanRepository;

        public BotAppService(IUserPlanRepository userPlanRepository)
        {
            _userPlanRepository = userPlanRepository;
        }

        public async Task<PagedResultDto<UsersInListWithBotFeatureDto>> GetUsersWithBotFeatureAsync()
        {
            var usersWithBotFeature = await _userPlanRepository.GetUsersWithBotFeatureAsyn();

            var usersList = ObjectMapper.Map<List<IdentityUser>, List<UsersInListWithBotFeatureDto>>(usersWithBotFeature);

            return new PagedResultDto<UsersInListWithBotFeatureDto>(usersList.Count, usersList);
        }
    }
}
