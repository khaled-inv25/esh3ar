using Esh3arTech.Messages;
using Microsoft.AspNetCore.Authorization;
using System;
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
            var messageQueryable = await _messageRepository.GetQueryableAsync();

            var total = messageQueryable.Count(msg => msg.CreatorId.Equals(CurrentUser.Id!.Value));

            var thisMonth = messageQueryable
                .Where(msg => msg.CreationTime >= CalculateAnchor(DateAnchor.StartMonth) && msg.CreationTime <= Clock.Now)
                .Count(msg => msg.CreatorId.Equals(CurrentUser.Id!.Value));

            var thisWeek = messageQueryable
                .Where(msg => msg.CreationTime >= CalculateAnchor(DateAnchor.StartWeek) && msg.CreationTime <= Clock.Now)
                .Count(msg => msg.CreatorId.Equals(CurrentUser.Id!.Value));

            var today = messageQueryable
                .Where(msg => msg.CreationTime >= CalculateAnchor(DateAnchor.StartToday) && msg.CreationTime <= Clock.Now)
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

        private DateTime CalculateAnchor(DateAnchor anchor)
        {
            var date = Clock.Now;

            return anchor switch
            {
                DateAnchor.StartMonth => Clock.Now.AddDays(-date.Day + 1).Date,
                DateAnchor.StartWeek => Clock.Now.AddDays(-((int)date.DayOfWeek + 1) % 7).Date,
                DateAnchor.StartToday => Clock.Now.Date,
                _ => date
            };
        }
    }
}
