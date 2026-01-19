using Esh3arTech.Messages.Delivery;
using Esh3arTech.Messages;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using System.Linq;

namespace Esh3arTech.BackgroundJobs
{
    public class SendMessageFromUiWithAttachmentJob : AsyncBackgroundJob<SendMessageFromUiWithAttachmentArg>, ITransientDependency
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        private readonly IMessageRepository _messageRepository;
        private readonly IMessageDeliveryService _messageDeliveryService;

        public SendMessageFromUiWithAttachmentJob(
            IMessageRepository messageRepository,
            IMessageDeliveryService messageDeliveryService)
        {
            _messageRepository = messageRepository;
            _messageDeliveryService = messageDeliveryService;
        }

        public override async Task ExecuteAsync(SendMessageFromUiWithAttachmentArg args)
        {
            await _semaphore.WaitAsync();

            try
            {
                var attachment = args.Attachments.First();
                var message = Message.CreateOneWayMessageWithAttachment(
                    args.Id,
                    args.CreatorId!.Value,
                    attachment.Id,
                    args.RecipientPhoneNumber,
                    args.MessageContent,
                    attachment.FileName,
                    attachment.Type,
                    attachment.AccessUrl);

                await _messageRepository.InsertAsync(message);

                await _messageDeliveryService.DeliverMessageAsync(new DeliverMessageDto
                {
                    Id = message.Id,
                    RecipientPhoneNumber = message.RecipientPhoneNumber,
                    MessageContent = message.MessageContent,
                    CreatorId = message.CreatorId!.Value,
                    AccessUrl = attachment.AccessUrl ?? string.Empty,
                    UrlExpiresAt = attachment.UrlExpiresAt,
                });
            }
            finally
            {
                _semaphore.Release();
            }

        }
    }
}
