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
    public class BatchMessageIngestionWorker : BackgroundService, IBackgroundWorker
    {
        private const int BatchIntervalMs = 100;

        private readonly IHighThroughputBatchMessageBuffer _highThroughputBatchMessageBuffer;
        private readonly IMessageRepository _messageRepository;
        private readonly IMessageDeliveryService _messageDeliveryService;

        public BatchMessageIngestionWorker(
            IHighThroughputBatchMessageBuffer highThroughputBatchMessageBuffer,
            IMessageRepository messageRepository,
            IMessageDeliveryService messageDeliveryService)
        {
            _highThroughputBatchMessageBuffer = highThroughputBatchMessageBuffer;
            _messageRepository = messageRepository;
            _messageDeliveryService = messageDeliveryService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var reader = _highThroughputBatchMessageBuffer.Reader;

            while (!stoppingToken.IsCancellationRequested)
            {
                if (await reader.WaitToReadAsync(stoppingToken))
                {
                    await Task.Delay(BatchIntervalMs, stoppingToken);

                    while (reader.TryRead(out var messages))
                    {
                        if (messages.Count != 0)
                        {
                            await ProcessBatchAsync(messages);
                        }
                    }
                }
            }
        }

        private async Task ProcessBatchAsync(List<Message> msgs)
        {
            await _messageRepository.BulkInsertMessages(msgs);

            var serlizedMessages = JsonSerializer.Serialize(msgs);
            await _messageDeliveryService.DeliverBatchMessageAsync(new DeliverBatchMessageDto { JsonMessages = serlizedMessages });
        }
    }
}
