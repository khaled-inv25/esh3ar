using Esh3arTech.Abp.Blob.Services;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

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
            var blobExists = await container.ExistsAsync("blobName");
            if (!blobExists)
            {
                return NotFound();
            }
            var stream = await container.GetAsync(id);
            return Ok(new RemoteStreamContent(stream));
        }

        [HttpGet]
        public async Task<IActionResult> String()
        {
            return Ok("Test");
        }
    }
}
