namespace Neurix.BLL.Configuration
{
    /// <summary>
    /// Bound from the "Crm:SeedAdmin" configuration section. Keeps the BLL from
    /// reaching into IConfiguration with magic strings scattered through it.
    /// </summary>
    public class CrmSeedAdminOptions
    {
        public const string SectionName = "Crm:SeedAdmin";

        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Explicit opt-in for seeding the administrator outside Development.
        /// Defaults to false: a production deployment must never gain a
        /// credentials-backed admin account just because the section exists.
        /// </summary>
        public bool AllowInProduction { get; set; }
    }
}
