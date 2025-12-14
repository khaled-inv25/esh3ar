using Esh3arTech.MobileUsers;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Web.MobileUsers
{
    public class OnlineMobileUserManager : IOnlineMobileUserManager, ITransientDependency
    {
        private readonly OnlineUserTrackerService _onlineUserTrackerService;

        public OnlineMobileUserManager(OnlineUserTrackerService onlineUserTrackerService)
        {
            _onlineUserTrackerService = onlineUserTrackerService;
        }

        public async Task<bool> IsConnectedAsync(string mobileNumber)
        {
            return await _onlineUserTrackerService.IsConnectedAsync(mobileNumber);
        }
    }
}
