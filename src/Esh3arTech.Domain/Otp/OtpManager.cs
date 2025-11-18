using Esh3arTech.Settings;
using OtpNet;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.Settings;

namespace Esh3arTech.Otp
{
    public class OtpManager : DomainService
    {
        private readonly ISettingProvider _settingProvider;

        public OtpManager(ISettingProvider settingProvider)
        {
            _settingProvider = settingProvider;
        }

        public string Generate(ref string? secret, int keyLength = 20, int codeTimeout = 300, DateTime? timestamp = null)
        {
            byte[] key;

            if (string.IsNullOrWhiteSpace(secret))
            {
                key = KeyGeneration.GenerateRandomKey(keyLength);
                secret = Base32Encoding.ToString(key);
            }
            else
            {
                key = Base32Encoding.ToBytes(secret);
            }

            var otp = new Totp(key, step: codeTimeout);
            string code;

            if (timestamp.HasValue)
            {
                code = otp.ComputeTotp(timestamp.Value);
            }
            else
            {
                code = otp.ComputeTotp();
            }

            return code;
        }

        public async Task<string> BuildMessage(string otp)
        {
            var smsTemplate = await _settingProvider.GetOrNullAsync(Esh3arTechSettings.Otp.VerificationTemplate);

            // To send valid message template
            if (string.IsNullOrWhiteSpace(smsTemplate))
            {
                smsTemplate = "<#> Your Esh3arTech code is: {0}\r\nmaidILDtEQp8";
            }

            return string.Format(smsTemplate, otp);
        }
        
        public bool Verify(string code, string secret, int codeTimeout)
        {
            var key = Base32Encoding.ToBytes(secret);
            var otp = new Totp(key, step: codeTimeout);

            long tm;

            var result = otp.VerifyTotp(code, out tm);

            var totp = otp.ComputeTotp();

            return result;
        }

    }
}
