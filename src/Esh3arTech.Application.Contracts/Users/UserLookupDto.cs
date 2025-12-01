using System;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.Users
{
    public class UserLookupDto : EntityDto<Guid>
    {
        //public string Id { get; set; }

        public string UserName { get; set; }
    }
}
