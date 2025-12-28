using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;

namespace Esh3arTech.Data.Users
{
    public class UserDataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly IdentityUserManager _identityUserManager;
        private readonly IGuidGenerator _guidGenerator;

        public UserDataSeedContributor(IdentityUserManager identityUserManager, IGuidGenerator guidGenerator)
        {
            _identityUserManager = identityUserManager;
            _guidGenerator = guidGenerator;
        }

        public async Task SeedAsync(DataSeedContext context)
        {

            var exists = await _identityUserManager.FindByEmailAsync("967775265496@esh3artech.ebs") != null ||
                         await _identityUserManager.FindByEmailAsync("967775265497@esh3artech.ebs") != null ||
                         await _identityUserManager.FindByEmailAsync("967775265498@esh3artech.ebs") != null ||
                         await _identityUserManager.FindByEmailAsync("967775265499@esh3artech.ebs") != null;

            if (exists)
            {
                return;
            }

            var userA = new IdentityUser(
                    id: _guidGenerator.Create(),
                    userName: "esh3ar_userA",
                    email: $"esh3ar_userA@esh3artech.ebs"
                );
            userA.SetPhoneNumber("967775265494", true);

            await _identityUserManager.CreateAsync(userA);

            var userB = new IdentityUser(
                    id: _guidGenerator.Create(),
                    userName: "esh3ar_userB",
                    email: $"esh3ar_userB@esh3artech.ebs"
                );
            userB.SetPhoneNumber("967775265495", true);

            await _identityUserManager.CreateAsync(userB);

            var identityUser1 = new IdentityUser(
                    id: _guidGenerator.Create(),
                    userName: "967775265496",
                    email: $"967775265496@esh3artech.ebs"
                );
            identityUser1.SetPhoneNumber("967775265496", true);

            await _identityUserManager.CreateAsync(identityUser1);
            
            var identityUser2 = new IdentityUser(
                    id: _guidGenerator.Create(),
                    userName: "967775265497",
                    email: $"967775265497@esh3artech.ebs"
                );
            identityUser2.SetPhoneNumber("967775265497", true);

            await _identityUserManager.CreateAsync(identityUser2);
            
            var identityUser3 = new IdentityUser(
                    id: _guidGenerator.Create(),
                    userName: "967775265498",
                    email: $"967775265498@esh3artech.ebs"
                );
            identityUser3.SetPhoneNumber("967775265498", true);

            await _identityUserManager.CreateAsync(identityUser3);
            
            var identityUser4 = new IdentityUser(
                    id: _guidGenerator.Create(),
                    userName: "967775265499",
                    email: $"967775265499@esh3artech.ebs"
                );
            identityUser4.SetPhoneNumber("967775265499", true);

            await _identityUserManager.CreateAsync(identityUser4);
        }
    }
}
