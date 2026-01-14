using Esh3arTech.Messages;
using Esh3arTech.Web.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.Content;

namespace Esh3arTech.Web.Pages.UploadFiles
{
    public class UploadFileModalModel : AbpPageModel
    {
        [BindProperty]
        public UploadFileModel File { get; set; }

        private readonly IMessageAppService _messageAppService;

        public UploadFileModalModel(IMessageAppService messageAppService)
        {
            _messageAppService = messageAppService;
        }

        public void OnGet()
        {
        }

        public async Task OnPost() 
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

                throw new UserFriendlyException(error!);
            }

            using var stream = File.ImportFile.OpenReadStream();
            var msgsInFile = new RemoteStreamContent(
                stream,
                File.ImportFile.FileName,
                File.ImportFile.ContentType);

            await _messageAppService.SendMessagesFromFile(msgsInFile);
        }
    }

    public class UploadFileModel
    {
        [Required]
        [DataType(DataType.Upload)]
        [MaxFileSize(MessageConts.MaxFileSize)]
        [AllowedExtensions([".xlsx", ".csv", ".json"])]
        public IFormFile ImportFile { get; set; }
    }
}
