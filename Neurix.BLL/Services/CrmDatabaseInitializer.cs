using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Neurix.DAL.Data;

namespace Neurix.BLL.Services
{
    public class CrmDatabaseInitializer : ICrmDatabaseInitializer
    {
        private readonly CrmDbContext _db;
        private readonly ICrmSeeder _seeder;
        private readonly ILogger<CrmDatabaseInitializer> _logger;

        public CrmDatabaseInitializer(
            CrmDbContext db,
            ICrmSeeder seeder,
            ILogger<CrmDatabaseInitializer> logger)
        {
            _db = db;
            _seeder = seeder;
            _logger = logger;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _db.Database.MigrateAsync(cancellationToken);
                await _seeder.SeedAsync();

                _logger.LogInformation("CRM database is up to date.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "CRM database migration/seeding failed. The CRM will be unavailable until the database is reachable; the public site is unaffected.");
            }
        }
    }
}
