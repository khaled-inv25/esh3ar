using Esh3arTech.Messages;
using Esh3arTech.MobileUsers;
using Microsoft.EntityFrameworkCore;

namespace Esh3arTech
{
    public static class Esh3arTechDbContextModelCreatingExtensions
    {
        public static void ConfigureEsh3arTech(this ModelBuilder builder)
        {
            builder.Entity<EtTempMobileUserData>(temp => 
            {
                temp.ToTable("#TempMobileUsers", t => t.ExcludeFromMigrations());
                temp.HasNoKey();
            });
        }
    }
}
