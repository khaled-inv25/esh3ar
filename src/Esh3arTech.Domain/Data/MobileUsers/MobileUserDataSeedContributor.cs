using Esh3arTech.MobileUsers;
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Esh3arTech.Data.MobileUsers
{
    public class MobileUserDataSeedContributor : IDataSeedContributor, ITransientDependency
    {

        private readonly IRepository<MobileUser, Guid> _mobileUserRepository;

        public MobileUserDataSeedContributor(IRepository<MobileUser, Guid> mobileUserRepository)
        {
            _mobileUserRepository = mobileUserRepository;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if (await _mobileUserRepository.GetCountAsync() > 0)
            {
                return;
            }

            var mobileUser1 = new MobileUser(
                Guid.NewGuid(),
                "967775265496",
                false
            );
            mobileUser1.Status = MobileUserRegisterStatus.Verified;
            
            var mobileUser2 = new MobileUser(
                Guid.NewGuid(),
                "967775265497",
                false
            );
            mobileUser2.Status = MobileUserRegisterStatus.Verified;
            
            var mobileUser3 = new MobileUser(
                Guid.NewGuid(),
                "967775265498",
                false
            );
            mobileUser3.Status = MobileUserRegisterStatus.Verified;
            
            var mobileUser4 = new MobileUser(
                Guid.NewGuid(),
                "967775265499",
                false
            );
            mobileUser4.Status = MobileUserRegisterStatus.Verified;


            await _mobileUserRepository.InsertManyAsync([mobileUser1, mobileUser2, mobileUser3, mobileUser4]);


        }
    }
}
