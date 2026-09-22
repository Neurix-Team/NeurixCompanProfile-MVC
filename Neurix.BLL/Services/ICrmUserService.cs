using Neurix.BLL.Dtos;

namespace Neurix.BLL.Services
{
    /// <summary>
    /// Reads and updates the signed-in CRM user's own account. Deliberately no
    /// role, email or lockout management here — those are owner/admin operations,
    /// not self-service.
    /// </summary>
    public interface ICrmUserService
    {
        Task<CrmUserProfile?> GetProfileAsync(Guid userId);

        Task<CrmProfileUpdateOutcome> UpdateFullNameAsync(Guid userId, string fullName);

        Task<CrmPasswordChangeOutcome> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    }
}
