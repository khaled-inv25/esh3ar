using Esh3arTech.Messages;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace Esh3arTech.UserMessages
{
    public class UserMessagesAppService : Esh3arTechAppService, IUserMessagesAppService
    {
        private readonly IMessageRepository _messageRepository;

        public UserMessagesAppService(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        [Authorize]
        public async Task<MessagesStatusDto> GetMessagesStatus()
        {
            var messageQuerable = await _messageRepository.GetQueryableAsync();

            var date = Clock.Now;
            var startMonth = Clock.Now.AddDays(-date.Day + 1).Date;
            var startWeek = Clock.Now.AddDays(-(int)date.DayOfWeek).Date;

            var total = messageQuerable.Count(msg => msg.CreatorId.Equals(CurrentUser.Id!.Value));

            var thisMonth = messageQuerable
                .Where(msg => msg.CreationTime <= date && msg.CreationTime >= startMonth)
                .Count(msg => msg.CreatorId.Equals(CurrentUser.Id!.Value));

            var thisWeek = messageQuerable
                .Where(msg => msg.CreationTime <= date && msg.CreationTime >= startWeek)
                .Count(msg => msg.CreatorId.Equals(CurrentUser.Id!.Value));

            var today = messageQuerable
                .Where(msg => msg.CreationTime <= date)
                .Count(msg => msg.CreatorId.Equals(CurrentUser.Id!.Value));


            var messagesStatus = new MessagesStatusDto
            {
                TotalMessages = total,
                TotalMessageThisMonth = thisMonth,
                TotalMessageThisWeek = thisWeek,
                TotalMessageToday = today
            };

            return messagesStatus;
        }
    }
}
