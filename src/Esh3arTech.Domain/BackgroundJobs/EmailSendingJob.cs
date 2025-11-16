using Esh3arTech.SmsProviders;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Esh3arTech.BackgroundJobs
{
    public class EmailSendingJob : AsyncBackgroundJob<EmailSendingArgs>, ITransientDependency
    {
        private readonly ISmsSenderFactory _smsSenderFactory;
        private readonly ISettingProvider _settingProvider;

        public EmailSendingJob(
            ISmsSenderFactory smsSenderFactory,
            ISettingProvider settingProvider)
        {
            _smsSenderFactory = smsSenderFactory;
            _settingProvider = settingProvider;
        }

        public override async Task ExecuteAsync(EmailSendingArgs args)
        {
            var provider = _smsSenderFactory.Create("email");
            await provider.SendSmsAsync(args.EmailAddress, args.Body, _settingProvider);
        }
    }
}
