using JetBrains.Annotations;
using Volo.Abp.Content;

namespace Esh3arTech.Messages
{
    public class SendOneWayMessageWithAttachmentFromUiDto : MessageBaseDto
    {
        [CanBeNull]
        public IRemoteStreamContent ImageStreamContent { get; set; }
    }
}
