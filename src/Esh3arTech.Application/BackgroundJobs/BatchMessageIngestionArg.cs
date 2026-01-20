using Esh3arTech.Messages;
using System.Collections.Generic;

namespace Esh3arTech.BackgroundJobs
{
    public class BatchMessageIngestionArg
    {
        public List<EnqueueBatchMessageDto> EnqueueMessages { get; set; }
    }
}
