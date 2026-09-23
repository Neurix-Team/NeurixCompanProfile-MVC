using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neurix.BLL.Services.Cms;
using Neurix.DAL.Data;

namespace Neurix.BLL.DependencyInjection
{
    /// <summary>
    /// Composition root for the CMS module.
    /// Wires up data access, migrations history table, and CMS business services.
    /// </summary>
    public static class CmsServiceCollectionExtensions
    {
        public static IServiceCollection AddCms(this IServiceCollection services, IConfiguration configuration)
        {
            // ── Data access (DAL) ──
            // Use dedicated "CmsDb" connection string if supplied; fallback to "CrmDb" for unified database instances.
            var connectionString = configuration.GetConnectionString("CmsDb");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = configuration.GetConnectionString("CrmDb");
            }

            services.AddSingleton<CmsContentRevision>();
            services.AddDbContext<CmsDbContext>(options =>
            {
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    options.UseSqlServer(connectionString, sql =>
                    {
                        // Separate migrations history table prevents collisions with CRM migrations
                        sql.MigrationsHistoryTable("__EFMigrationsHistory_CMS");
                    });
                }
            });

            // ── Business services (BLL) ──
            services.AddScoped<ICmsCompanyProfileService, CmsCompanyProfileService>();
            services.AddScoped<ICmsServiceService, CmsServiceService>();
            services.AddScoped<ICmsSocialLinkService, CmsSocialLinkService>();
            services.AddScoped<ICmsSiteSettingService, CmsSiteSettingService>();
            services.AddScoped<ICmsTeamMemberService, CmsTeamMemberService>();
            services.AddScoped<ICmsBlogPostService, CmsBlogPostService>();
            services.AddScoped<ICmsTestimonialService, CmsTestimonialService>();
            services.AddScoped<ICmsProjectService, CmsProjectService>();
            services.AddScoped<ICmsDivisionPageService, CmsDivisionPageService>();
            services.AddScoped<ICmsMediaAssetService, CmsMediaAssetService>();
            services.AddScoped<ICmsHomeSectionService, CmsHomeSectionService>();
            services.AddScoped<ICmsPageBandService, CmsPageBandService>();
            services.AddScoped<ICmsContentPageService, CmsContentPageService>();
            services.AddScoped<ICmsMenuItemService, CmsMenuItemService>();
            services.AddScoped<ICmsSeeder, CmsSeeder>();
            services.AddScoped<ICmsDatabaseInitializer, CmsDatabaseInitializer>();

            return services;
        }

        /// <summary>
        /// Migrates and seeds the CMS database.
        /// Fail-soft by design: never crashes the host application on database unavailability.
        /// </summary>
        public static async Task InitializeCmsDatabaseAsync(this IServiceProvider services)
        {
            await using var scope = services.CreateAsyncScope();
            var initializer = scope.ServiceProvider.GetRequiredService<ICmsDatabaseInitializer>();
            await initializer.InitializeAsync();
        }
    }
}
