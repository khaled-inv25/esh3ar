using Esh3arTech.Messages.Buffer;
using Esh3arTech.Messages.Eto;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.EventBus.Distributed;

namespace Esh3arTech.Messages.BackgroundWorkers
{
    public class MessageIngestionWorker : BackgroundService, IBackgroundWorker
    {
        private readonly IHighThroughputMessageBuffer _highThroughputMessageBuffer;
        private const int BatchIntervalMs = 100;
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly IMessageRepository _messageRepository;

        public MessageIngestionWorker(
            IHighThroughputMessageBuffer highThroughputMessageBuffer,
            IDistributedEventBus distributedEventBus,
            IMessageRepository messageRepository)
        {
            _highThroughputMessageBuffer = highThroughputMessageBuffer;
            _distributedEventBus = distributedEventBus;
            _messageRepository = messageRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var reader = _highThroughputMessageBuffer.Reader;
            while (!stoppingToken.IsCancellationRequested)
            {
                if (await reader.WaitToReadAsync(stoppingToken))
                {
                    await Task.Delay(BatchIntervalMs, stoppingToken);

                    var batch = new List<Message>();

                    while (reader.TryRead(out var message))
                    {
                        batch.Add(message);
                    }

                    if (batch.Count != 0)
                    {
                        await ProcessBatchAsync(batch);
                    }
                }
            }
        }

        private async Task ProcessBatchAsync(List<Message> msgs)
        {
            await _messageRepository.InsertManyAsync(msgs);

            var serlizedMessages = JsonSerializer.Serialize(msgs);
            await _distributedEventBus.PublishAsync(new MessageIngestionEto() { JsonMessages = serlizedMessages });
        }
    }
}