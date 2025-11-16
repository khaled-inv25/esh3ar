using Volo.Abp.BackgroundJobs;

namespace Esh3arTech.BackgroundJobs
{
    [BackgroundJobName("emails")]
    public class EmailSendingArgs
    {
        public string EmailAddress { get; set; }
        public string Body { get; set; }
    }
}
