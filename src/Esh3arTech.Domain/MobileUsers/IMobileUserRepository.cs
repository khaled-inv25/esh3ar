using Esh3arTech.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Esh3arTech.MobileUsers
{
    public interface IMobileUserRepository : IRepository<MobileUser, Guid>
    {
        Task<List<string>> CheckExistanceOrVerifiedMobileNumberAsync(List<EtTempMobileUserData> batch);
    }
}
