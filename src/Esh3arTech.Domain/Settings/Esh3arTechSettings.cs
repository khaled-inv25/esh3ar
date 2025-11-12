namespace Esh3arTech.Settings;

public static class Esh3arTechSettings
{
    private const string Prefix = "Et";
    private const string NetPrefix = "Net";

    public static class Otp
    {
        public const string VerificationTemplate = Prefix + ".Sms.VerificationTemplate";
        public const string VerificationTimeout = Prefix + ".Sms.OtpTimeout";
        public const string KeyLength = Prefix + ".Sms.KeyLength";
    }

    public static class Whatsapp
    {
        public const string BaseUrl = NetPrefix + ".Whatsapp.BaseUrl";

        public const string UserName = NetPrefix + ".Whatsapp.UserName";

        public const string Password = NetPrefix + ".Whatsapp.Password";

        public const string ClientId = NetPrefix + ".Whatsapp.ClientId";

        public const string ClientSecret = NetPrefix + ".Whatsapp.ClientSecret";
    }

    public static class Email
    {
        public const string Sender = NetPrefix + ".Email.Sender";
        public const string From = NetPrefix + ".Email.From";
        public const string SmtpHost = NetPrefix + ".Email.SmtpHost";
        public const string Port = NetPrefix + ".Email.SmtpPort";
        public const string Password = NetPrefix + ".Email.Password";
    }

    public static class Registretion
    {
        public const string SendOtpToStaticMobileNumber = Prefix + ".Registration.SendOtpToStaticMobileNumber";
    }
}
