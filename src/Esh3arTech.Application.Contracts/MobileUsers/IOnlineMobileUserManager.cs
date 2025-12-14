using System.Threading.Tasks;

namespace Esh3arTech.MobileUsers
{
    public interface IOnlineMobileUserManager
    {
        Task<bool> IsConnectedAsync(string mobileNumber);
    }
}
