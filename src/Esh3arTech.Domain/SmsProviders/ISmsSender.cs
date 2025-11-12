using System.Threading.Tasks;
using Volo.Abp.Settings;

namespace Esh3arTech.SmsProviders
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string to, string message, ISettingProvider _settingProvider);
    }
}
