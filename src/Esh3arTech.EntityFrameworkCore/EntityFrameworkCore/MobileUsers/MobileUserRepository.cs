using EFCore.BulkExtensions;
using Esh3arTech.Messages;
using Esh3arTech.MobileUsers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Esh3arTech.EntityFrameworkCore.MobileUsers
{
    public class MobileUserRepository : EfCoreRepository<Esh3arTechDbContext, MobileUser, Guid>, IMobileUserRepository
    {
        public MobileUserRepository(IDbContextProvider<Esh3arTechDbContext> dbContextProvider) 
            : base(dbContextProvider)
        {
        }

        public async Task<List<string>> CheckExistanceOrVerifiedMobileNumberAsync(List<EtTempMobileUserData> batch)
        {
            var missingNumbers = await CheckIfAnyMissingNumbersAsync(batch);

            return missingNumbers;
        }

        private async Task<List<string>> CheckIfAnyMissingNumbersAsync(List<EtTempMobileUserData> tempItems)
        {
            foreach (var item in tempItems)
            {
                if (!item.MobileNumber.StartsWith("967"))
                {
                    item.MobileNumber = "967" + item.MobileNumber;
                }
            }

            var dbContext = await GetDbContextAsync();
            var connection =  dbContext.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await dbContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE #TempMobileUsers
                (
                    Id INT IDENTITY(1, 1) PRIMARY KEY,
                    MobileNumber NVARCHAR(15) NOT NULL
                )"
            );

            await dbContext.BulkInsertAsync(tempItems, new BulkConfig()
            {
                CustomDestinationTableName = "#TempMobileUsers"
            });

            var missingNumbers = await dbContext.Database.SqlQueryRaw<string>(@"
                SELECT DISTINCT v.MobileNumber 
                FROM #TempMobileUsers v
                LEFT JOIN EtMobileUsers mu ON mu.MobileNumber = v.MobileNumber
                WHERE mu.Id IS NULL OR mu.Status != 1").ToListAsync();

            return missingNumbers;
        }
    }
}
