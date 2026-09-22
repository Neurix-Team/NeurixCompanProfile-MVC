using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neurix.BLL.Configuration;
using Neurix.BLL.Services;
using Neurix.DAL.Data;
using Neurix.DAL.Models;

namespace Neurix.BLL.DependencyInjection
{
    /// <summary>
    /// The CRM's composition root. Everything data- and identity-related is wired
    /// up here so the Web layer's Program.cs never names CrmDbContext, EF Core,
    /// or the Identity stores.
    /// </summary>
    public static class CrmServiceCollectionExtensions
    {
        public static IServiceCollection AddCrm(this IServiceCollection services, IConfiguration configuration)
        {
            // ── Data access (DAL) ──
            // An empty connection string still registers the provider, so the failure
            // only shows up later as "The ConnectionString property has not been
            // initialized" on the first query. Skipping the call keeps that consistent
            // with AddCms and makes the misconfiguration explicit at startup.
            var connectionString = configuration.GetConnectionString("CrmDb");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.Error.WriteLine(
                    "[Neurix CRM] WARNING: 'ConnectionStrings:CrmDb' is not configured. " +
                    "The public site will still run, but CRM/CMS sign-in and all dashboard " +
                    "features will be unavailable until a connection string is supplied.");
            }

            services.AddDbContext<CrmDbContext>(options =>
            {
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    options.UseSqlServer(connectionString);
                }
            });

            // ── Identity (cookie based) ──
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
                .AddEntityFrameworkStores<CrmDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<CrmSeedAdminOptions>(
                configuration.GetSection(CrmSeedAdminOptions.SectionName));

            // ── Business services (BLL) ──
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<ILeadService, LeadService>();
            services.AddScoped<ILeadConversionService, LeadConversionService>();
            services.AddScoped<IDealService, DealService>();
            services.AddScoped<ICrmAuthService, CrmAuthService>();
            services.AddScoped<ICrmUserService, CrmUserService>();
            services.AddScoped<INewsletterService, NewsletterService>();
            services.AddScoped<ICrmSeeder, CrmSeeder>();
            services.AddScoped<ICrmDatabaseInitializer, CrmDatabaseInitializer>();

            return services;
        }

        /// <summary>
        /// Migrates and seeds the CRM database. Call once at startup.
        /// Never throws — see <see cref="CrmDatabaseInitializer"/>.
        /// </summary>
        public static async Task InitializeCrmDatabaseAsync(this IServiceProvider services)
        {
            await using var scope = services.CreateAsyncScope();
            var initializer = scope.ServiceProvider.GetRequiredService<ICrmDatabaseInitializer>();
            await initializer.InitializeAsync();
        }
    }
}
