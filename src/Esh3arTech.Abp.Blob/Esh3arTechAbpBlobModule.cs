using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.Modularity;

namespace Esh3arTech.Abp.Blob
{
    [DependsOn(
        typeof(AbpBlobStoringFileSystemModule)
        )]
    public class Esh3arTechAbpBlobModule : AbpModule
    {
    }
}
