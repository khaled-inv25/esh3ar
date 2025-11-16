using Esh3arTech.MobileUsers;
using Esh3arTech.SmsProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;
using Volo.Abp.Uow;
using static Esh3arTech.Settings.Esh3arTechSettings;

namespace Esh3arTech.Settings;

public class Esh3arTechSettingDefinitionProvider : SettingDefinitionProvider
{
    private readonly IConfiguration _appConfiguration;

    public Esh3arTechSettingDefinitionProvider(IConfiguration appConfiguration)
    {
        _appConfiguration = appConfiguration;
    }

    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(Esh3arTechSettings.Otp.VerificationTemplate, GetFromSettings(Esh3arTechSettings.Otp.VerificationTemplate.Replace(".", ":"))),
            new SettingDefinition(Esh3arTechSettings.Otp.CodeTimeout, GetFromSettings(Esh3arTechSettings.Otp.CodeTimeout.Replace(".", ":"), "300")),
            new SettingDefinition(Esh3arTechSettings.Otp.KeyLength, GetFromSettings(Esh3arTechSettings.Otp.KeyLength.Replace(".", ":"), "20")),
            new SettingDefinition(Registretion.SendOtpToStaticMobileNumber, GetFromSettings(Registretion.SendOtpToStaticMobileNumber.Replace(".", ":"), "false")),
            new SettingDefinition(Email.Sender, GetFromSettings(Email.Sender.Replace(".", ":")) ?? ""),
            new SettingDefinition(Email.From, GetFromSettings(Email.From.Replace(".", ":")) ?? ""),
            new SettingDefinition(Email.SmtpHost, GetFromSettings(Email.SmtpHost.Replace(".", ":")) ?? ""),
            new SettingDefinition(Email.Password, GetFromSettings(Email.Password.Replace(".", ":")) ?? "")
            );
    }

    private string? GetFromSettings(string name, string? defaultValue = null)
    {
        return _appConfiguration[name] ?? defaultValue;
    }
}
