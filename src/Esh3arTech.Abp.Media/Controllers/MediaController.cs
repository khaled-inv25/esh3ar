using Esh3arTech.Abp.Blob.Services;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;

namespace Esh3arTech.Abp.Media.Controllers
{
    [Route("api/media")]
    [ApiController]
    public class MediaController : AbpController
    {
        private readonly IBlobService _blobService;

        public MediaController(IBlobService blobService)
        {
            _blobService = blobService;
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadAsync(string id)
        {
            var container = await _blobService.GetContainerAsync();

            var blobExists = await container.ExistsAsync(id);
            if (!blobExists)
            {
                return NotFound();
            }

            var memoryStream = new MemoryStream();

            using (var fileStream = await container.GetAsync(id))
            {
                await fileStream.CopyToAsync(memoryStream);
            }

            memoryStream.Position = 0;

            if (!await _blobService.DeleteBlobAsync(id))
            {
                // log warning.
            }

            return Ok(new RemoteStreamContent(memoryStream));
        }
    }
}
