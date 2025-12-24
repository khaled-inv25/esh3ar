using Esh3arTech.MobileUsers;
using Esh3arTech.Plans;
using Microsoft.Extensions.Configuration;
using System;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Settings;
using Volo.Abp.Users;

namespace Esh3arTech.Messages.SendBehavior
{
    public class MessageFactory : IMessageFactory, ITransientDependency
    {
        private readonly IRepository<MobileUser, Guid> _mobileUserRepository;
        private readonly UserPlanManager _userPlanManager;
        private readonly ICurrentUser _currentUser;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ISettingProvider _settingProvider;

        public MessageFactory(
            IRepository<MobileUser, Guid> mobileUserRepository,
            UserPlanManager userPlanManager,
            ICurrentUser currentUser,
            IGuidGenerator guidGenerator,
            ISettingProvider settingProvider)
        {
            _mobileUserRepository = mobileUserRepository;
            _userPlanManager = userPlanManager;
            _currentUser = currentUser;
            _guidGenerator = guidGenerator;
            _settingProvider = settingProvider;
        }

        public IOneWayMessageManager Create(MessageType type)
        {
            return type switch
            {
               MessageType.OneWay => new OneWayMessageManager(_mobileUserRepository, _userPlanManager, _currentUser, _guidGenerator, _settingProvider),
                _ => throw new ArgumentException($"{Esh3arTechDomainErrorCodes.UnsupportedMessageType}: {type}")
            };
        }
    }
}
