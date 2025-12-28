//using Esh3arTech.Messages;
//using Esh3arTech.Messages.Eto;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using Volo.Abp.BackgroundWorkers;
//using Volo.Abp.Domain.Repositories;
//using Volo.Abp.EventBus.Distributed;
//using Volo.Abp.Linq;
//using Volo.Abp.Threading;
//using Volo.Abp.Uow;

//namespace Esh3arTech.EntityFrameworkCore.BackgroundWorkers
//{
//    public class MessageRetryWorker : AsyncPeriodicBackgroundWorkerBase
//    {
//        public MessageRetryWorker(
//            AbpAsyncTimer timer,
//            IServiceScopeFactory serviceScopeFactory)
//            : base(timer, serviceScopeFactory)
//        {
//            Timer.Period = 30000; // 30 seconds
//        }

//        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
//        {
//            var logger = workerContext.ServiceProvider.GetRequiredService<ILogger<MessageRetryWorker>>();
//            var unitOfWorkManager = workerContext.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
//            var messageRepository = workerContext.ServiceProvider.GetRequiredService<IRepository<Message, Guid>>();
//            var eventBus = workerContext.ServiceProvider.GetRequiredService<IDistributedEventBus>();
//            var asyncExecuter = workerContext.ServiceProvider.GetRequiredService<IAsyncQueryableExecuter>();

//            using (var uow = unitOfWorkManager.Begin())
//            {
//                try
//                {
//                    var queryable = await messageRepository.GetQueryableAsync();
                    
//                    var messagesToRetry = await asyncExecuter.ToListAsync(
//                        queryable
//                            .Where(m => m.Status == MessageStatus.Failed || m.Status == MessageStatus.Queued)
//                            .Where(m => m.NextRetryAt != null && m.NextRetryAt <= DateTime.UtcNow)
//                            .Take(100)); // Process up to 100 messages per cycle

//                    if (messagesToRetry.Any())
//                    {
//                        logger.LogInformation("Found {Count} messages ready for retry", messagesToRetry.Count);

//                        foreach (var message in messagesToRetry)
//                        {
//                            try
//                            {
//                                // Mark as retrying and republish
//                                message.MarkAsRetrying();
//                                await messageRepository.UpdateAsync(message);

//                                var sendMsgEto = new SendOneWayMessageEto
//                                {
//                                    Id = message.Id,
//                                    RecipientPhoneNumber = message.RecipientPhoneNumber,
//                                    MessageContent = message.MessageContent,
//                                    Subject = message.Subject,
//                                    IdempotencyKey = message.IdempotencyKey,
//                                    Priority = message.Priority
//                                };

//                                await eventBus.PublishAsync(sendMsgEto);
                                
//                                logger.LogDebug("Requeued message {MessageId} for retry", message.Id);
//                            }
//                            catch (Exception ex)
//                            {
//                                logger.LogError(ex, "Error requeuing message {MessageId} for retry", message.Id);
//                            }
//                        }
//                    }

//                    await uow.CompleteAsync();
//                }
//                catch (Exception ex)
//                {
//                    logger.LogError(ex, "Error in MessageRetryWorker");
//                }
//            }
//        }
//    }
//}

