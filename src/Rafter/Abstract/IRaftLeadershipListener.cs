using Rafter.Values;
using System.Threading;
using System.Threading.Tasks;

namespace Rafter.Abstract;

public interface IRaftLeadershipListener
{
    Task LeadershipAcquiredAsync(CancellationToken cancellationToken);
    Task LeadershipLostAsync(PeerInfo currentLeader, CancellationToken cancellationToken);
}
