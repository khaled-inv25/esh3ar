using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Data;

/* This is used if database provider does't define
 * IEsh3arTechDbSchemaMigrator implementation.
 */
public class NullEsh3arTechDbSchemaMigrator : IEsh3arTechDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
