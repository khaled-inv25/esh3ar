using Esh3arTech.BackgroundJobs;
using Esh3arTech.MobileUsers;
using Esh3arTech.Otp;
using Esh3arTech.Registretions;
using Esh3arTech.Settings;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Settings;
using static Esh3arTech.Utility.MobileNumberPreparator;

namespace Esh3arTech.Registrations
{
    public class RegistretionAppService : Esh3arTechAppService, IRegistretionAppService
    {
        private readonly IRepository<MobileUser, Guid> _mobileUserRepository;
        private readonly IRepository<RegistretionRequest, Guid> _registretionRequestRepository;
        private readonly ISettingProvider _settingProvider;
        private readonly OtpManager _otpManager;
        private readonly MobileUserManager _mobileUserManager;
        private readonly RegistretionRequestManager _registretionRequestManager;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IdentityUserManager _identityUserManager;


        public RegistretionAppService(
            IRepository<MobileUser, Guid> mobileUserRepository,
            ISettingProvider settingProvider,
            OtpManager otpManager,
            MobileUserManager mobileUserManager,
            RegistretionRequestManager registretionRequestManager,
            IRepository<RegistretionRequest, Guid> registretionRequestRepository,
            IBackgroundJobManager backgroundJobManager,
            IdentityUserManager identityUserManager)
        {
            _mobileUserRepository = mobileUserRepository;
            _settingProvider = settingProvider;
            _otpManager = otpManager;
            _mobileUserManager = mobileUserManager;
            _registretionRequestManager = registretionRequestManager;
            _registretionRequestRepository = registretionRequestRepository;
            _backgroundJobManager = backgroundJobManager;
            _identityUserManager = identityUserManager;
        }

        public async Task<RegisterOutputDto> RegisterAsync(RegisterRequestDto input)
        {
            var mobileNumber = PrepareMobileNumber(input.MobileNumber);

            #region Prepare

            string? secret = null;
            var otpTimeout = await _settingProvider.GetAsync<int>(Esh3arTechSettings.Otp.CodeTimeout);
            var keyLength = await _settingProvider.GetAsync<int>(Esh3arTechSettings.Otp.KeyLength);

            var generatedOtp = _otpManager.Generate(ref secret, keyLength, otpTimeout);
            var message = await _otpManager.BuildMessage(generatedOtp);

            #endregion

            var mobileUser = await _mobileUserRepository.FirstOrDefaultAsync(m => m.MobileNumber == mobileNumber);
            RegistretionRequest? registretionRequest;

            if (mobileUser is null)
            {
                mobileUser = _mobileUserManager.CreateMobileUser(mobileNumber);
                await _mobileUserRepository.InsertAsync(mobileUser);
                registretionRequest = await _registretionRequestManager.CreateRegistretionRequestAsync(input.OS, secret!, mobileUser.Id);
                await _registretionRequestRepository.InsertAsync(registretionRequest!);
            }
            else
            {
                registretionRequest = await _registretionRequestManager.GetLastNotVerifiedRequest(mobileUser);
                if (registretionRequest is null)
                {
                    generatedOtp = _otpManager.Generate(ref secret, keyLength, otpTimeout);
                    message = await _otpManager.BuildMessage(generatedOtp);
                    registretionRequest = await _registretionRequestManager.CreateRegistretionRequestAsync(input.OS, secret!, mobileUser.Id);
                    await _registretionRequestRepository.InsertAsync(registretionRequest!);
                }
            }

            // Send TOTP.
            await _backgroundJobManager.EnqueueAsync(new EmailSendingArgs() { EmailAddress = "khaled.inv25@gmail.com", Body = message });

            return new RegisterOutputDto(registretionRequest!.Id);
        }

        public async Task<VerifedDto> VerifyOtpAsync(VerifyOtpRequestDto input)
        {
            var minutes = await _settingProvider.GetAsync<int>(Esh3arTechSettings.Otp.CodeTimeout);

            var requestResult = await _registretionRequestRepository.WithDetailsAsync(r => r.MobileUser!)
                ?? throw new UserFriendlyException(message: "The registration request is not exists!");

            var registretionRequest = await AsyncExecuter.SingleAsync(requestResult.Where(r => r.Id == input.RegistrationRequestId));

            var registrationRequest = await _registretionRequestManager.GetLastNotVerifiedRequest(registretionRequest!.MobileUser!)
                ?? throw new BusinessException("No valid code — Request a new code");

            if (!_otpManager.Verify(input.OtpCode, registrationRequest.Secret, minutes))
            {
                throw new BusinessException("No valid code — Request a new code");
            }

            registrationRequest.SetAsVerified(Clock.Now);
            registrationRequest!.MobileUser!.Status = MobileUserRegisterStatus.Verified;
            await _mobileUserRepository.UpdateAsync(registrationRequest.MobileUser);

            var identityUser = await _identityUserManager.FindByEmailAsync($"{registrationRequest.MobileUser.MobileNumber}@esh3artech.ebs");

            //IdentityUser newMobilIdentityUser;

            if (identityUser is null)
            {
                var newMobilIdentityUser = new IdentityUser(
                    id: GuidGenerator.Create(),
                    userName: registrationRequest.MobileUser.MobileNumber,
                    email: $"{registrationRequest.MobileUser.MobileNumber}@esh3artech.ebs"
                );

                newMobilIdentityUser.SetPhoneNumber(registrationRequest.MobileUser.MobileNumber, true);

                var result = await _identityUserManager.CreateAsync(newMobilIdentityUser);

                if (!result.Succeeded)
                {
                    throw new UserFriendlyException("Failed to create identity user for mobile user.");
                }

                return new VerifedDto(await _identityUserManager.GenerateUserTokenAsync(newMobilIdentityUser, TokenOptions.DefaultProvider, "PhoneVerification"));
            }

            return new VerifedDto("no token is provided!");
        }

        public async Task ResendOtpAsync(ResendOtpRequestDto input)
        {
            var mobileNumber = PrepareMobileNumber(input.MobileNumber);

            var registrationRequest = await _registretionRequestManager.GetLastNotVerifiedRequest(await _mobileUserRepository.GetAsync(m => m.MobileNumber == mobileNumber)) 
                ?? throw new UserFriendlyException("No valid registration request found!");

            var secret = registrationRequest.Secret;
            var otp = _otpManager.Generate(ref secret);
            var message = await _otpManager.BuildMessage(otp);

            // Re-send the same TOTP.
            await _backgroundJobManager.EnqueueAsync(new EmailSendingArgs() { EmailAddress = "khaled.inv25@gmail.com", Body = message });

        }
    }
}
