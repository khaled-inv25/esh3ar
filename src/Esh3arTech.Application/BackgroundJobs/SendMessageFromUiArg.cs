using Esh3arTech.Messages;
using Volo.Abp.BackgroundJobs;

namespace Esh3arTech.BackgroundJobs
{
    [BackgroundJobName("SendMessageFromUiArg")]
    public class SendMessageFromUiArg
    {
        public Message Message { get; set; }
    }
}
