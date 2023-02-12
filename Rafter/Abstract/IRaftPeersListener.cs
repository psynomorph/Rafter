using Rafter.Values;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rafter.Abstract;

public interface IRaftPeersListener
{
    Task OnPeersUpdate(IReadOnlyCollection<PeerInfo> peers, CancellationToken cancellationToken);
}
