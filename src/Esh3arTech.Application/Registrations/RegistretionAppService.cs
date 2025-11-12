using Esh3arTech.MobileUsers;
using Esh3arTech.Otp;
using Esh3arTech.Settings;
using Esh3arTech.SmsProviders;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Settings;
using static Esh3arTech.Settings.Esh3arTechSettings;
using static Esh3arTech.Utility.MobileNumberPreparator;

namespace Esh3arTech.Registrations
{
    public class RegistretionAppService : Esh3arTechAppService, IRegistretionAppService
    {
        private readonly IRepository<MobileUser, Guid> _mobileUserRepository;
        private readonly ISettingProvider _settingProvider;
        private readonly OtpManager _otpManager;
        private readonly MobileUserManagment _mobileUserManager;
        private readonly ISmsSenderFactory _smsSenderFactory;

        public RegistretionAppService(
            IRepository<MobileUser, Guid> mobileUserRepository,
            ISettingProvider settingProvider,
            OtpManager otpManager,
            MobileUserManagment mobileUserManager,
            ISmsSenderFactory smsSenderFactory)
        {
            _mobileUserRepository = mobileUserRepository;
            _settingProvider = settingProvider;
            _otpManager = otpManager;
            _mobileUserManager = mobileUserManager;
            _smsSenderFactory = smsSenderFactory;
        }

        public async Task<RegisterOutputDto> RegisterAsync(RegisterRequestDto input)
        {
            var mobileNumber = PrepareMobileNumber(input.MobileNumber);

            #region Prepare

            var sendOtpToStaticMobileNumber = await _settingProvider.GetAsync<bool>(Registretion.SendOtpToStaticMobileNumber);
            string? secret = null;
            var otpTimeout = await _settingProvider.GetAsync<int>(Esh3arTechSettings.Otp.VerificationTimeout);
            var keyLength = await _settingProvider.GetAsync<int>(Esh3arTechSettings.Otp.KeyLength);

            var generatedOtp = _otpManager.Generate(ref secret, keyLength, otpTimeout);
            var message = await _otpManager.BuildMessage(generatedOtp);

            #endregion

            var mobileUser = await _mobileUserRepository.FirstOrDefaultAsync(m => m.MobileNumber == mobileNumber);

            if (mobileUser is null)
            {
                mobileUser = _mobileUserManager.CreateMobileUser(mobileNumber);
                await _mobileUserRepository.InsertAsync(mobileUser);
            }
            else
            {

            }

            // Send OTP.
            var provider = _smsSenderFactory.Create(input.HowToSendOtp);
            await provider.SendSmsAsync("khaled.inv25@gmail.com", message, _settingProvider);

            return null;
        }
    }
}
