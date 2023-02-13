using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Rafter.Values;
using Rafter.Abstract;
using System.Threading;
using System.Linq;
using System;

namespace Rafter.Impl;

internal sealed class PeersStorage : IRaftPeersListener
{
    private readonly ConcurrentDictionary<PeerId, PeerState> _peers = new();
    private PeerId? _currentPeerId;

    public int QuorumSize { get; private set; }

    public void SetCurrentPeerId(PeerId currentPeerId)
    {
        _currentPeerId = currentPeerId;
    }

    public void UpdatePeers(IEnumerable<PeerInfo> peers)
    {
        var activePeersCount = 0;

        foreach(var peerInfo in peers)
        {
            if (peerInfo.IsActive)
            {
                activePeersCount++;
            }

            _peers.AddOrUpdate(peerInfo.PeerId,
                _ => new PeerState(peerInfo),
                (_, peer) => { peer.Update(peerInfo); return peer; });
            ;
        }

        QuorumSize = activePeersCount / 2 + 1;
    }

    public IReadOnlyCollection<PeerState> GetAllPeers() => (IReadOnlyCollection<PeerState>)_peers.Values;

    public IReadOnlyCollection<PeerState> GetOtherActivePeers() => _peers.Values
        .Where(peer => peer.Id != _currentPeerId && peer.IsActive)
        .ToList();

    public PeerState GetPeer(PeerId peerId)
    {
        if (!_peers.TryGetValue(peerId, out var peer))
        {
            throw new ArgumentException();
        }

        return peer;
    }

    Task IRaftPeersListener.OnPeersUpdate(IReadOnlyCollection<PeerInfo> peers, CancellationToken cancellationToken)
    {
        UpdatePeers(peers);
        return Task.CompletedTask;
    }
}
