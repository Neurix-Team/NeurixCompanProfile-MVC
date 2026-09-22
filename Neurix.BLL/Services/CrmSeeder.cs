using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Neurix.BLL.Common;
using Neurix.BLL.Configuration;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services
{
    /// <summary>
    /// Seeds CRM roles and the initial administrator. Was a static helper called
    /// from Program.cs; now an injectable service so the Web layer no longer has
    /// to resolve RoleManager/UserManager itself.
    /// </summary>
    public class CrmSeeder : ICrmSeeder
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CrmSeedAdminOptions _options;
        private readonly IHostEnvironment _environment;

        public CrmSeeder(
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IOptions<CrmSeedAdminOptions> options,
            IHostEnvironment environment)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _options = options.Value;
            _environment = environment;
        }

        public async Task SeedAsync()
        {
            foreach (var roleName in CrmRoles.All)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var roleResult = await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Failed to create CRM role '{roleName}': {Describe(roleResult)}");
                    }
                }
            }

            // No credentials configured (e.g. production before secrets are supplied) — roles only.
            if (string.IsNullOrWhiteSpace(_options.Email) || string.IsNullOrWhiteSpace(_options.Password))
            {
                return;
            }

            // A credentials-backed admin is a development convenience. Any other
            // environment (Staging, Production, custom names) must opt in explicitly
            // through Crm:SeedAdmin:AllowInProduction — never seeded just because the
            // section happens to be configured.
            if (!_environment.IsDevelopment() && !_options.AllowInProduction)
            {
                return;
            }

            if (await _userManager.FindByEmailAsync(_options.Email) is not null)
            {
                return;
            }

            var admin = new ApplicationUser
            {
                UserName = _options.Email,
                Email = _options.Email,
                EmailConfirmed = true,
                FullName = string.IsNullOrWhiteSpace(_options.FullName)
                    ? "CRM Administrator"
                    : _options.FullName
            };

            var createResult = await _userManager.CreateAsync(admin, _options.Password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create the seed CRM administrator: {Describe(createResult)}");
            }

            var roleAssignment = await _userManager.AddToRoleAsync(admin, CrmRoles.Admin);
            if (!roleAssignment.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to assign the '{CrmRoles.Admin}' role to the seed administrator: {Describe(roleAssignment)}");
            }
        }

        private static string Describe(IdentityResult result) =>
            string.Join("; ", result.Errors.Select(e => e.Description));
    }
}
