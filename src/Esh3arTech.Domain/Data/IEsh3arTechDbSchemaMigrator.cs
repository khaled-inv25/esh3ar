using System.Threading.Tasks;

namespace Esh3arTech.Data;

public interface IEsh3arTechDbSchemaMigrator
{
    Task MigrateAsync();
}
