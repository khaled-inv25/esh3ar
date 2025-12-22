using Esh3arTech.Messages;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

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

        public void OnGetAsync()
        {
        }

        public async Task OnPostAsync()
        {
            var model = ObjectMapper.Map<SendMessageViewModal, SendOneWayMessageDto>(Model);

            await _messageAppService.SendOneWayMessageAsync(model);
        }
    }

    public class SendMessageViewModal
    {
        [RegularExpression(@"^(77|78|70|73|71)\d{7}$")]
        [Length(maximumLength: MessageConts.MaxRecipientNumberLength, minimumLength: MessageConts.MaxRecipientNumberLength)]
        public string RecipientPhoneNumber { get; set; }

        [Required]
        public string MessageContent { get; set; }

        public string Subject { get; set; } = "Manual Message";
    }
}
