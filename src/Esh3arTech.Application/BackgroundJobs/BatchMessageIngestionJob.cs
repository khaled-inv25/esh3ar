using Esh3arTech.Messages;
using Esh3arTech.Messages.Delivery;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.BackgroundJobs
{
    public class BatchMessageIngestionJob : AsyncBackgroundJob<BatchMessageIngestionArg>, ITransientDependency
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        private readonly IMessageRepository _messageRepository;
        private readonly IMessageDeliveryService _messageDeliveryService;

        public BatchMessageIngestionJob(
            IMessageRepository messageRepository,
            IMessageDeliveryService messageDeliveryService)
        {
            _messageRepository = messageRepository;
            _messageDeliveryService = messageDeliveryService;
        }
        public override async Task ExecuteAsync(BatchMessageIngestionArg args)
        {
            await _semaphore.WaitAsync();
            try
            {
                await _messageRepository.BulkInsertMessagesAsync(args.Messages);

                await _messageDeliveryService.DeliverBatchMessageAsync( new DeliverBatchMessageDto 
                {
                    JsonMessages = SerializeMessages(args.Messages)
                });
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private string SerializeMessages(List<Message> messages)
        {
            return JsonSerializer.Serialize(messages);
        }
    }
}
