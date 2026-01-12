using Esh3arTech.Messages.Buffer;
using Esh3arTech.Messages.Delivery;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;

namespace Esh3arTech.Messages.BackgroundWorkers
{
    public class MessageIngestionWorker : BackgroundService, IBackgroundWorker
    {
        private const int BatchIntervalMs = 100;

        private readonly IHighThroughputMessageBuffer _highThroughputMessageBuffer;
        private readonly IMessageRepository _messageRepository;
        private readonly IMessageDeliveryService _messageDeliveryService;

        public MessageIngestionWorker(
            IHighThroughputMessageBuffer highThroughputMessageBuffer,
            IMessageRepository messageRepository,
            IMessageDeliveryService messageDeliveryService)
        {
            _highThroughputMessageBuffer = highThroughputMessageBuffer;
            _messageRepository = messageRepository;
            _messageDeliveryService = messageDeliveryService;
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
            await _messageDeliveryService.DeliverBatchMessageAsync(new DeliverBatchMessageDto { JsonMessages = serlizedMessages });
        }
    }
}