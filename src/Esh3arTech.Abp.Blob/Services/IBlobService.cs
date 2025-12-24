using Volo.Abp.BlobStoring;
using Volo.Abp.Content;

namespace Esh3arTech.Abp.Blob.Services
{
    public interface IBlobService
    {
        Task SaveToFileSystemAsync(string base64, string fileName);
        Task SaveToFileSystemAsync(IRemoteStreamContent streamContent, string blobName);
        Task<IBlobContainer> GetContainerAsync();
        Task<bool> DeleteBlobAsync(string blobName);
    }
}
