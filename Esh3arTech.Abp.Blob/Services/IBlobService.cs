using Volo.Abp.Content;

namespace Esh3arTech.Abp.Blob.Services
{
    public interface IBlobService
    {
        Task SaveAsync(IRemoteStreamContent file, string fileName);
    }
}
