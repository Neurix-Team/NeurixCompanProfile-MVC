using System.Threading;
using System.Threading.Tasks;

namespace Neurix.BLL.Services.Cms
{
    public interface ICmsDatabaseInitializer
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);
    }
}
