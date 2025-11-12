using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Esh3arTech.Data;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.EntityFrameworkCore;

public class EntityFrameworkCoreEsh3arTechDbSchemaMigrator
    : IEsh3arTechDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreEsh3arTechDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the Esh3arTechDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<Esh3arTechDbContext>()
            .Database
            .MigrateAsync();
    }
}
