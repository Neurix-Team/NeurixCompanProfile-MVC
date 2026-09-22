using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Neurix.DAL.Data
{
    /// <summary>
    /// Lets `dotnet ef migrations` scaffold CRM migrations without a real connection
    /// string: the app deliberately registers no SQL provider while
    /// "ConnectionStrings:CrmDb" is empty (fail-fast misconfiguration), which would
    /// otherwise make design-time tooling impossible in a clean checkout.
    /// </summary>
    public class CrmDbContextDesignTimeFactory : IDesignTimeDbContextFactory<CrmDbContext>
    {
        public CrmDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<CrmDbContext>()
                .UseSqlServer("Server=.;Database=NeurixCRM;Trusted_Connection=True;")
                .Options;

            return new CrmDbContext(options);
        }
    }
}
