using System.Security.Claims;

namespace Neurix.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// The signed-in CRM user's id, taken from the claim Identity issues at sign-in.
        /// Returns <see cref="Guid.Empty"/> when unauthenticated — CRM controllers are
        /// behind [Authorize], so in practice this always resolves.
        /// </summary>
        public static Guid GetUserId(this ClaimsPrincipal principal)
        {
            var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }
    }
}
