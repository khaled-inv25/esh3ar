using System.ComponentModel.DataAnnotations;
using Volo.Abp.Content;

namespace Esh3arTech.Messages
{
    public class SendOneWayMessageWithAttachmentFromUiDto : MessageBaseDto
    {
        [Required]
        public IRemoteStreamContent ImageStreamContent { get; set; }
    }
}
