using Volo.Abp.BlobStoring;
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

        public async Task SaveAsync(string base64, string fileName)
        {
            var fileBytes = Convert.FromBase64String(base64);
            await using var stream = new MemoryStream(fileBytes);
            await _container.SaveAsync(fileName, stream, overrideExisting: false);
        }
    }
}
