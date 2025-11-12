using Microsoft.Extensions.DependencyInjection;
using System;
using Volo.Abp.DependencyInjection;
using static Esh3arTech.Settings.Esh3arTechSettings;

namespace Esh3arTech.SmsProviders
{
    public class SmsSenderFactory : ISmsSenderFactory, ITransientDependency
    {
        private readonly IServiceProvider _serviceProvider;

        public SmsSenderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ISmsSender Create(string provider)
        {
            return provider.ToLower() switch
            {
                "email" => _serviceProvider.GetRequiredService<EmailSender>(),
                _ => throw new ArgumentException($"{Esh3arTechDomainErrorCodes.UnsupportedSMSProvider}: {provider}")
            };
        }
    }
}
