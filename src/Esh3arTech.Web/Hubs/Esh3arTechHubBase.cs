using Volo.Abp.AspNetCore.SignalR;

namespace Esh3arTech.Web.Hubs
{
    public abstract class Esh3arTechHubBase : AbpHub
    {
        protected abstract object? GetUserInfo();

        protected abstract void IsAuthorized(string number);
    }
}
