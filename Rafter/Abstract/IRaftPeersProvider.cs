using Rafter.Values;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rafter.Abstract;

public interface IRaftPeersProvider
{
    Task<IReadOnlyCollection<PeerInfo>> GetCurrentPeersAsync(CancellationToken cancellationToken);
    IDisposable AddListener(IRaftPeersListener listener);
}
