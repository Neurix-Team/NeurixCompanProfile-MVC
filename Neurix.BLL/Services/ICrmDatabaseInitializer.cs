namespace Neurix.BLL.Services
{
    /// <summary>
    /// Brings the CRM database up to date at startup (migrate, then seed).
    /// Deliberately fail-soft: the public marketing site must stay available
    /// even when the CRM database is unreachable.
    /// </summary>
    public interface ICrmDatabaseInitializer
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);
    }
}
