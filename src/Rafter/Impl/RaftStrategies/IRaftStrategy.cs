using System.Threading;
using System.Threading.Tasks;

namespace Rafter.Impl.RaftStrategies;

internal interface IRaftStrategy
{
    Task RunAsync(CancellationToken cancellationToken);
}
