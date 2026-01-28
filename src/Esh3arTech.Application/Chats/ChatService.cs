using Esh3arTech.Messages.SendBehavior;
using Esh3arTech.MobileUsers;
using Esh3arTech.Utility;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace Esh3arTech.Chats
{
    public class ChatService : IChatService, ITransientDependency
    {
        private readonly IMobileUserRepository _mobileUserRepository;
        private readonly IIdentityUserRepository _identityUserRepository;
        private readonly IMessageFactory _messageFactory;

        public ChatService(
            IMobileUserRepository mobileUserRepository,
            IIdentityUserRepository identityUserRepository, 
            IMessageFactory messageFactory)
        {
            _mobileUserRepository = mobileUserRepository;
            _identityUserRepository = identityUserRepository;
            _messageFactory = messageFactory;
        }

        public async Task<ReceiveToBusinessMessageDto> CreateMobileToBusinessMessageAsync(ReceiveToBusinessMessageDto input)
        {
            var businessMobileNumber = MobileNumberPreparator.PrepareMobileNumber(input.MobileAccount);


            return input;
        }

        public async Task<ReceiveToMobileMessageDto> CreateBusinessToMobileMessageAsync(ReceiveToMobileMessageDto input)
        {
            return input;
        }
    }
}
