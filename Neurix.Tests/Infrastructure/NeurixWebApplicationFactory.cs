using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neurix.DAL.Data;

namespace Neurix.Tests.Infrastructure
{
    /// <summary>
    /// Boots the real ASP.NET Core pipeline (routing, model binding, anti-forgery,
    /// auth, status codes) for HTTP-level tests, backed by isolated EF Core InMemory
    /// databases instead of the real SQL Server connection string in appsettings.json.
    /// Each instance gets its own database names, so tests never share state.
    /// </summary>
    public class NeurixWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _crmDbName = $"crm-{System.Guid.NewGuid()}";
        private readonly string _cmsDbName = $"cms-{System.Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureAppConfiguration((_, config) =>
            {
                // Defense in depth: even though the DbContext registrations below are
                // replaced, make sure nothing in the pipeline can open the real
                // production connection string committed in appsettings.json.
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:CrmDb"] = null,
                    ["ConnectionStrings:CmsDb"] = null,
                });
            });

            builder.ConfigureServices(services =>
            {
                ReplaceDbContext<CrmDbContext>(services, _crmDbName);
                ReplaceDbContext<CmsDbContext>(services, _cmsDbName);
            });
        }

        // The app registers CrmDbContext/CmsDbContext with UseSqlServer() before this
        // ConfigureWebHost callback runs, which leaves SqlServer's provider services in
        // the shared IServiceCollection. Simply re-adding AddDbContext(UseInMemoryDatabase)
        // on top throws "Only a single database provider can be registered" because EF's
        // default internal service provider is built once per IServiceCollection and sees
        // both providers. Giving the InMemory context its own dedicated internal service
        // provider avoids that shared resolution entirely.
        private static readonly IServiceProvider InMemoryProvider =
            new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();

        private static void ReplaceDbContext<TContext>(IServiceCollection services, string dbName)
            where TContext : DbContext
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<TContext>(options =>
                options.UseInMemoryDatabase(dbName).UseInternalServiceProvider(InMemoryProvider));
        }

        /// <summary>
        /// InitializeCrmDatabaseAsync/InitializeCmsDatabaseAsync call Database.MigrateAsync(),
        /// which throws (and is swallowed, fail-soft) against the InMemory provider. Call this
        /// once per test to actually build the InMemory schema from the EF model.
        /// </summary>
        public void EnsureDatabasesCreated()
        {
            using var scope = Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<CrmDbContext>().Database.EnsureCreated();
            scope.ServiceProvider.GetRequiredService<CmsDbContext>().Database.EnsureCreated();
        }
    }
}
