using Esh3arTech.Messages;
using Microsoft.AspNetCore.Mvc;
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
        public string RecipientPhoneNumber = "775265496";

        public string MessageContent { get; set; }

        public string Subject { get; set; } = "Test Subject";
    }
}
