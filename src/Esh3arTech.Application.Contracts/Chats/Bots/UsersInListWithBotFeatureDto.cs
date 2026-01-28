using System;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.Chats.Bots
{
    public class UsersInListWithBotFeatureDto : EntityDto<Guid>
    {
        public string UserName { get; set; }
    }
}
