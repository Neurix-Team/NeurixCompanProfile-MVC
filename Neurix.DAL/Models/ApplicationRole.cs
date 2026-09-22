using Microsoft.AspNetCore.Identity;

namespace Neurix.DAL.Models
{
    /// <summary>
    /// CRM role. Two roles exist in the MVP: Admin and CrmStaff (see CrmSeeder).
    /// </summary>
    public class ApplicationRole : IdentityRole<Guid>
    {
    }
}
