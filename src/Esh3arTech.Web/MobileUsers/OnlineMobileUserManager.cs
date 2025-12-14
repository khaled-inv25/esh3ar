using Esh3arTech.MobileUsers;
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

        public bool IsConnected(string mobileNumber)
        {
            return _onlineUserTrackerService.IsConnected(mobileNumber);
        }
    }
}
