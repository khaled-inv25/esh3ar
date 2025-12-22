using Volo.Abp.Content;

namespace Esh3arTech.Abp.Blob.Services
{
    public interface IBlobService
    {
        Task SaveAsync(string base64, string fileName);
    }
}
