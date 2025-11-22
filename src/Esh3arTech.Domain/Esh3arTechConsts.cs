using Volo.Abp.Identity;

namespace Esh3arTech;

public static class Esh3arTechConsts
{
    public const string DbTablePrefix = "Et";

    public const string? DbSchema = null;
    public const string AdminEmailDefaultValue = IdentityDataSeedContributor.AdminEmailDefaultValue;
    public const string AdminPasswordDefaultValue = IdentityDataSeedContributor.AdminPasswordDefaultValue;

    public const string TblMobileUsers = DbTablePrefix + "MobileUsers";
    public const string TblRegistretionRequest = DbTablePrefix + "RegistretionRequest";
    
    public const string TblUserPlan = DbTablePrefix + "UserPlans";

    public const string DefaultOtpSubject = "OTP";
}
