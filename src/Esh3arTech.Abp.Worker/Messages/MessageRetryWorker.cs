using Esh3arTech.Messages;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Threading;

namespace Esh3arTech.Abp.Worker.Messages
{
    public class MessageRetryWorker : AsyncPeriodicBackgroundWorkerBase
    {
        private const int PeriodInMilliseconds = 30000;

        private readonly IMessageRepository _messageRepository;
        private readonly IDistributedEventBus _distributedEventBus;

        public MessageRetryWorker(
            AbpAsyncTimer timer,
            IServiceScopeFactory serviceScopeFactory,
            IMessageRepository messageRepository,
            IDistributedEventBus distributedEventBus)
            : base(timer, serviceScopeFactory)
        {
            Timer.Period = PeriodInMilliseconds;
            _messageRepository = messageRepository;
            _distributedEventBus = distributedEventBus;
        }

        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            await ReQueueMessage();
        }

        private async Task ReQueueMessage()
        {
            var messagesToRetry = await _messageRepository.GetFailedOrQueuedMessagesAsync();

            if (messagesToRetry.Count != 0)
            {
                foreach (var msg in messagesToRetry)
                {
                    msg.MarkAsRetrying();
                    await _messageRepository.UpdateAsync(msg);

                    var sendMessageEto = new MessageRetryEto()
                    {
                        Id = msg.Id,
                        RecipientPhoneNumber = msg.RecipientPhoneNumber,
                        MessageContent = msg.MessageContent,
                        From = "msg.From",
                        Subject = msg.Subject,
                        AccessUrl = "AccessUrl",
                        UrlExpiresAt = null
                    };

                    await _distributedEventBus.PublishAsync(sendMessageEto);
                }
            }
        }
    }
}
