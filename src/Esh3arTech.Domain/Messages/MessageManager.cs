using Esh3arTech.Plans;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace Esh3arTech.Messages
{
    public class MessageManager : DomainService
    {
        private readonly UserPlanManager _userPlanManager;
        private readonly IConfiguration _configuration;

        public MessageManager(
            UserPlanManager userPlanManager, 
            IConfiguration configuration)
        {
            _userPlanManager = userPlanManager;
            _configuration = configuration;
        }

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

        public async Task<Message> CreateOneWayMessageWithAttachmentAsync(Guid userId, string recipientPhoneNumber, string? messageContent, string base64, ContentType type)
        {
            Message msgToReturn;
            if (string.IsNullOrEmpty(messageContent))
            {
                msgToReturn = await CreateOneWayMessage(userId, recipientPhoneNumber);
                msgToReturn.SetMessageContentOrNull();
            }
            else
            {
                msgToReturn = await CreateOneWayMessage(userId, recipientPhoneNumber);
                msgToReturn.SetMessageContentOrNull(messageContent);
            }

            msgToReturn.AddAttachment(GuidGenerator.Create(), type, $"{_configuration["Url"]!}", CalcBase64SizeInMb(base64));

            return msgToReturn;
        }

        private long CalcBase64SizeInMb(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                return 0;
            }

            int length = base64.Length;

            int padding = 0;

            if (base64.EndsWith("=="))
            {
                padding = 2;
            }
            else if (base64.EndsWith("="))
            {
                padding = 1;
            }

            long sizeInBytes = (long)(length * 3.0 / 4.0) - padding;
            long sizeInMb = (long)(sizeInBytes / (1024 * 1024));

            return sizeInMb;
        }
    }
}
