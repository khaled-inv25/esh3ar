using Esh3arTech.EntityFrameworkCore.Messages;
using Esh3arTech.EntityFrameworkCore.MobileUsers;
using Esh3arTech.EntityFrameworkCore.Plans;
using Esh3arTech.EntityFrameworkCore.Registrations;
using Esh3arTech.EntityFrameworkCore.Subscriptions;
using Esh3arTech.Messages;
using Esh3arTech.MobileUsers;
using Esh3arTech.Plans;
using Esh3arTech.Plans.Subscriptions;
using Esh3arTech.Registretions;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DistributedEvents;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace Esh3arTech.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class Esh3arTechDbContext :
    AbpDbContext<Esh3arTechDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    public DbSet<MobileUser> MobileUsers { get; set; }

    public DbSet<RegistretionRequest> RegistrationRequests { get; set; }

    public DbSet<UserPlan> UserPlans { get; set; }

    public DbSet<Subscription> Subscriptions { get; set; }

    public DbSet<SubscriptionRenewalHistory> SubscriptionRenewalHistories { get; set; }

    public DbSet<Message> Messages { get; set; }

    public DbSet<MessageAttachment> Attachments { get; set; }

    public DbSet<EtTempMobileUserData> TempMobileUsers { get; set; } // this is a temp entity for bulk operations

    #region Entities from the modules

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    // Event
    //public DbSet<OutgoingEventRecord> OutgoingEvents { get; set; }

    #endregion

    public Esh3arTechDbContext(DbContextOptions<Esh3arTechDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();
        builder.Entity<Tenant>().ConfigureExtraProperties();
        builder.ConfigureEsh3arTech();
        //builder.ConfigureEventOutbox();

        builder.ApplyConfiguration(new MobileUserConfiguration());
        builder.ApplyConfiguration(new RegistrationRequestConfiguration());
        builder.ApplyConfiguration(new UserPlanConfiguration());
        builder.ApplyConfiguration(new SubscriptionConfiguration());
        builder.ApplyConfiguration(new SubscriptionRenewalHistoryConfiguration());
        builder.ApplyConfiguration(new MessageConfiguration());
        builder.ApplyConfiguration(new AttachmentConfiguration());

    }
}
