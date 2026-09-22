namespace Neurix.BLL.Services
{
    /// <summary>
    /// Ensures the CRM roles and the initial administrator account exist.
    /// Safe to run on every startup — every step is idempotent.
    /// </summary>
    public interface ICrmSeeder
    {
        Task SeedAsync();
    }
}
