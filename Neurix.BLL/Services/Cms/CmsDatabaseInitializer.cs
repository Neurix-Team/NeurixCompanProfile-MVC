using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Neurix.DAL.Data;

namespace Neurix.BLL.Services.Cms
{
    public class CmsDatabaseInitializer : ICmsDatabaseInitializer
    {
        private readonly CmsDbContext _db;
        private readonly ICmsSeeder _seeder;
        private readonly ILogger<CmsDatabaseInitializer> _logger;

        public CmsDatabaseInitializer(
            CmsDbContext db,
            ICmsSeeder seeder,
            ILogger<CmsDatabaseInitializer> logger)
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

                _logger.LogInformation("CMS database is up to date and seeded.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "CMS database migration/seeding failed. The CMS will be unavailable until the database is reachable; the public site and CRM are unaffected.");
            }
        }
    }
}
