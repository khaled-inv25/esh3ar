using Esh3arTech.Messages;
using Esh3arTech.Web.Helpers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.Content;

namespace Esh3arTech.Web.Pages.Messages
{
    public class SendMessageModalModel : AbpPageModel
    {
        private readonly IMessageAppService _messageAppService;

        public SendMessageModalModel(IMessageAppService messageAppService)
        {
            _messageAppService = messageAppService;
        }

        [BindProperty]
        public SendMessageViewModal Model { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

                throw new UserFriendlyException(error!);
            }

            if (Model.ImageFile != null)
            {
                var dto = new SendOneWayMessageWithAttachmentFromUiDto()
                {
                    RecipientPhoneNumber = Model.RecipientPhoneNumber,
                    MessageContent = Model.MessageContent,
                };

                using var stream = Model.ImageFile.OpenReadStream();
                dto.ImageStreamContent = new RemoteStreamContent(
                    stream,
                    Model.ImageFile.FileName,
                    Model.ImageFile.ContentType);

                await _messageAppService.SendMessageWithAttachmentFromUiAsync(dto);
            }
            else
            {
                var model = ObjectMapper.Map<SendMessageViewModal, SendOneWayMessageDto>(Model);
                await _messageAppService.SendOneWayMessageAsync(model);
            }

            return NoContent();
        }
    }

    public class SendMessageViewModal
    {
        [RegularExpression(@"^(77|78|70|73|71)\d{7}$")]
        [Length(maximumLength: MessageConts.MobileNumberLength, minimumLength: MessageConts.MobileNumberLength)]
        [Required]
        public string RecipientPhoneNumber { get; set; }

        [CanBeNull]
        public string? MessageContent { get; set; }

        public string Subject { get; set; } = "Manual Message";

        [CanBeNull]
        [DataType(DataType.Upload)]
        [MaxFileSize(MessageConts.MaxFileSize)]
        [AllowedExtensions([".jpg", ".png", ".jpeg"])]
        public IFormFile? ImageFile { get; set; }
    }
}
