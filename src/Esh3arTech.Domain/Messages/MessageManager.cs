using Esh3arTech.Plans;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace Esh3arTech.Messages
{
    public class MessageManager : DomainService
    {
        private readonly UserPlanManager _userPlanManager;

        public MessageManager(UserPlanManager userPlanManager)
        {
            _userPlanManager = userPlanManager;
        }

        /*
        public async Task<Message> CreateMessageAsync(
            Guid userId,
            string recipientPhoneNumber,
            string subject,
            string messageContent)
        {
            await _userPlanManager.CanSendMessageAsync(userId);

            var msgToReturn = new Message(GuidGenerator.Create(), recipientPhoneNumber, subject);
            msgToReturn.SetMessageContent(messageContent);

            return msgToReturn;
        }
        */

        public async Task<Message> CreateOneWayMessage(Guid userId, string recipientPhoneNumber)
        {
            await _userPlanManager.CanSendMessageAsync(userId);

            var msgToReturn = new Message(
                GuidGenerator.Create(),
                recipientPhoneNumber,
                MessageType.OneWay
                );

            return msgToReturn;
        }
    }
}
