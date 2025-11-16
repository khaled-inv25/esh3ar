using Volo.Abp.Domain.Services;

namespace Esh3arTech.MobileUsers
{
    public class MobileUserManager : DomainService
    {
        

        public MobileUser CreateMobileUser(string number, bool isStatic = false)
        {
            return new MobileUser(GuidGenerator.Create(), number, isStatic);
        }
    }
}
