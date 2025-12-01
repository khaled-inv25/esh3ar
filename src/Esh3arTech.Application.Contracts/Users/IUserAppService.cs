using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esh3arTech.Users
{
    public interface IUserAppService
    {
        Task<List<UserLookupDto>> GetUserLookup();
    }
}
