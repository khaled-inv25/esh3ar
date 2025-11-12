using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using static Esh3arTech.Settings.Esh3arTechSettings;

namespace Esh3arTech.SmsProviders
{
    public class EmailSender : ISmsSender, ISingletonDependency
    {
        public async Task SendSmsAsync(string to, string message, ISettingProvider _settingProvider)
        {
            var sender = await _settingProvider.GetOrNullAsync(Email.Sender);
            var email = await _settingProvider.GetOrNullAsync(Email.From);
            var password = await _settingProvider.GetOrNullAsync(Email.Password);
            var host = await _settingProvider.GetOrNullAsync(Email.SmtpHost);

            var mailMessage = new MailMessage(email, to, Esh3arTechConsts.DefaultOtpSubject, $"{sender}:\n{message}");

            var smtpClient = new SmtpClient(host, 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(email, password)
            };

            await smtpClient.SendMailAsync(mailMessage);
        }

    }
}
