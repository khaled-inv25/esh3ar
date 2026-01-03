using Esh3arTech.Messages.Buffer;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace Esh3arTech.Abp.Worker.Messages
{
    public class MessageIngestionWorker : AsyncPeriodicBackgroundWorkerBase
    {
        private const int PeriodInMilliseconds = 15000;

        private readonly IMessageBuffer _messageBuffer;

        public MessageIngestionWorker(
            AbpAsyncTimer timer,
            IServiceScopeFactory serviceScopeFactory,
            IMessageBuffer messageBuffer)
            : base(timer, serviceScopeFactory)
        {
            Timer.Period = PeriodInMilliseconds;
            _messageBuffer = messageBuffer;
        }

        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            var msg = _messageBuffer.Reader.TryRead(out var messageBufferDto);
        }
    }
}
