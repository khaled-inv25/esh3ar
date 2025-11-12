using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Esh3arTech.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class Esh3arTechDbContextFactory : IDesignTimeDbContextFactory<Esh3arTechDbContext>
{
    public Esh3arTechDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        Esh3arTechEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<Esh3arTechDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new Esh3arTechDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Esh3arTech.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
