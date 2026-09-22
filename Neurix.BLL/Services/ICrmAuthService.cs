using System.Security.Claims;
using Neurix.BLL.Dtos;

namespace Neurix.BLL.Services
{
    /// <summary>
    /// CRM sign-in/sign-out. Wraps ASP.NET Identity so controllers never touch
    /// SignInManager/UserManager directly — the rules about lockout and about
    /// not revealing whether an account exists live here, not in the UI.
    /// </summary>
    public interface ICrmAuthService
    {
        bool IsSignedIn(ClaimsPrincipal principal);

        Task<CrmSignInOutcome> SignInAsync(string email, string password, bool rememberMe);

        Task SignOutAsync();

        /// <summary>
        /// Re-issues the current request's auth cookie with a fresh security stamp.
        /// Call this right after the signed-in user changes their own password —
        /// otherwise their own session is invalidated by the same stamp rotation
        /// that is meant to log out every *other* session.
        /// </summary>
        Task RefreshSignInAsync(Guid userId);
    }
}
