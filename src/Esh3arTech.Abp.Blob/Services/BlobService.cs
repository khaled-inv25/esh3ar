using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.Abp.Blob.Services
{
    public class BlobService : IBlobService, ITransientDependency
    {
        private readonly IBlobContainer _container;

        public BlobService(IBlobContainer container)
        {
            _container = container;
        }

        public async Task SaveToFileSystemAsync(string base64, string fileName)
        {
            var fileBytes = Convert.FromBase64String(base64);
            await using var stream = new MemoryStream(fileBytes);
            await _container.SaveAsync(fileName, stream, overrideExisting: false);
        }

        public async Task SaveToFileSystemAsync(IRemoteStreamContent streamContent, string blobName)
        {
            await _container.SaveAsync(blobName, streamContent.GetStream());
        }

        public async Task<IBlobContainer> GetContainerAsync()
        {
            return _container;
        }

        public async Task<bool> DeleteBlobAsync(string blobName)
        {
            return await _container.DeleteAsync(blobName);
        }
    }
}
