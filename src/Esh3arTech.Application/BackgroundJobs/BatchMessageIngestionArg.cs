using Esh3arTech.Messages;
using System.Collections.Generic;
using Volo.Abp.BackgroundJobs;

namespace Esh3arTech.BackgroundJobs
{
    [BackgroundJobName("IngestionBatchMessageArg")]
    public class BatchMessageIngestionArg
    {
        public List<Message> Messages { get; set; } = new();
    }
}
