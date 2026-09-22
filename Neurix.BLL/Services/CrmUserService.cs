using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Neurix.BLL.Dtos;
using Neurix.DAL.Models;

namespace Neurix.BLL.Services
{
    public class CrmUserService : ICrmUserService
    {
        private const int FullNameMaxLength = 200;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CrmUserService> _logger;

        public CrmUserService(UserManager<ApplicationUser> userManager, ILogger<CrmUserService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<CrmUserProfile?> GetProfileAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new CrmUserProfile
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToArray()
            };
        }

        public async Task<CrmProfileUpdateOutcome> UpdateFullNameAsync(Guid userId, string fullName)
        {
            var trimmed = fullName.Trim();

            if (trimmed.Length == 0 || trimmed.Length > FullNameMaxLength)
            {
                return CrmProfileUpdateOutcome.InvalidFullName;
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return CrmProfileUpdateOutcome.NotFound;
            }

            user.FullName = trimmed;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Profile update for user {UserId} failed: {Errors}",
                    userId, string.Join("; ", result.Errors.Select(e => e.Description)));
                return CrmProfileUpdateOutcome.Unavailable;
            }

            return CrmProfileUpdateOutcome.Updated;
        }

        public async Task<CrmPasswordChangeOutcome> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return CrmPasswordChangeOutcome.NotFound;
            }

            var correct = await _userManager.CheckPasswordAsync(user, currentPassword);
            if (!correct)
            {
                return CrmPasswordChangeOutcome.IncorrectCurrentPassword;
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                // Wrong current password already handled above; any remaining failure is
                // the new password not meeting the policy (length 8+ per AddCrm).
                _logger.LogInformation("Password change for user {UserId} rejected by policy: {Errors}",
                    userId, string.Join("; ", result.Errors.Select(e => e.Description)));
                return CrmPasswordChangeOutcome.InvalidNewPassword;
            }

            // Re-authenticate the user with the new password, refreshing the security
            // stamp claim; otherwise other cookies stamped with the old password keep working.
            await _userManager.UpdateSecurityStampAsync(user);

            return CrmPasswordChangeOutcome.Success;
        }
    }
}
