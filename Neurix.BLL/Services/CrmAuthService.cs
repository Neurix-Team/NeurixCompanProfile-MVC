using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Neurix.BLL.Dtos;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services
{
    public class CrmAuthService : ICrmAuthService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CrmAuthService> _logger;

        public CrmAuthService(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<CrmAuthService> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public bool IsSignedIn(ClaimsPrincipal principal) => _signInManager.IsSignedIn(principal);

        public async Task<CrmSignInOutcome> SignInAsync(string email, string password, bool rememberMe)
        {
            // Sign-in is the first request that actually opens a database connection —
            // the public site and the login GET are both fail-soft. Without this guard a
            // missing "CrmDb" connection string surfaces only as a generic 500 page.
            try
            {
                return await SignInCoreAsync(email, password, rememberMe);
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.Data.Common.DbException)
            {
                _logger.LogError(ex,
                    "CRM sign-in failed: the CRM database is unreachable. Check the " +
                    "'ConnectionStrings:CrmDb' setting for this environment.");
                return CrmSignInOutcome.Unavailable;
            }
        }

        private async Task<CrmSignInOutcome> SignInCoreAsync(string email, string password, bool rememberMe)
        {
            var user = await _userManager.FindByEmailAsync(email);

            // An unknown email and a wrong password deliberately return the same
            // outcome, so the caller cannot use this to enumerate accounts.
            if (user is null)
            {
                return CrmSignInOutcome.InvalidCredentials;
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, password, rememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return CrmSignInOutcome.Success;
            }

            return result.IsLockedOut
                ? CrmSignInOutcome.LockedOut
                : CrmSignInOutcome.InvalidCredentials;
        }

        public Task SignOutAsync() => _signInManager.SignOutAsync();

        public async Task RefreshSignInAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is not null)
            {
                await _signInManager.RefreshSignInAsync(user);
            }
        }
    }
}
