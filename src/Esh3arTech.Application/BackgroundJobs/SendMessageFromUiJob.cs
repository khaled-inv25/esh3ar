using Esh3arTech.Messages.Delivery;
using Esh3arTech.Messages;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace Esh3arTech.BackgroundJobs
{
    public class SendMessageFromUiJob : AsyncBackgroundJob<SendMessageFromUiArg>, ITransientDependency
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        private readonly IMessageRepository _messageRepository;
        private readonly IMessageDeliveryService _messageDeliveryService;

        public SendMessageFromUiJob(
            IMessageRepository messageRepository, 
            IMessageDeliveryService messageDeliveryService)
        {
            _messageRepository = messageRepository;
            _messageDeliveryService = messageDeliveryService;
        }

        public override async Task ExecuteAsync(SendMessageFromUiArg args)
        {
            await _semaphore.WaitAsync();

            try
            {
                await _messageRepository.InsertAsync(args.Message);

                await _messageDeliveryService.DeliverMessageAsync(new DeliverMessageDto
                {
                    Id = args.Message.Id,
                    RecipientPhoneNumber = args.Message.RecipientPhoneNumber,
                    MessageContent = args.Message.MessageContent,
                    CreatorId = args.Message.CreatorId!.Value,
                    AccessUrl = null,
                    UrlExpiresAt = null,
                });
            }
            finally
            {
                _semaphore.Release();
            }

        }
    }
}
