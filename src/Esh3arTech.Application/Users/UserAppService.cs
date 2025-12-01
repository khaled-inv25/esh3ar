using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace Esh3arTech.Users
{
    public class UserAppService : Esh3arTechAppService, IUserAppService
    {
        private readonly IIdentityUserRepository _identityUserRepository;

        public UserAppService(IIdentityUserRepository identityUserRepository)
        {
            _identityUserRepository = identityUserRepository;
        }

        public async Task<List<UserLookupDto>> GetUserLookup()
        {
            var users = await _identityUserRepository.GetListAsync();

            return ObjectMapper.Map<List<IdentityUser>, List<UserLookupDto>>(users);
        }
    }
}
