using Microsoft.AspNetCore.Identity;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// CRM user account. Extends ASP.NET Core Identity with the display name
    /// shown across the CRM (record owners, assignees, activity authors).
    /// </summary>
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FullName { get; set; } = string.Empty;
    }
}
