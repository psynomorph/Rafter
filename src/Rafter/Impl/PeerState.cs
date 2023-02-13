using Rafter.Values;
using System;
using System.Diagnostics;

namespace Rafter.Impl;

internal sealed class PeerState
{
    public PeerId Id { get; private set; }
    public Uri Uri { get; private set; }
    public bool IsActive { get; private set; }
    public LogIndex LastIndex { get; private set; } = LogIndex.Zero;

    public PeerInfo PeerInfo => new PeerInfo(Id, Uri, IsActive);

    public PeerState(PeerInfo peerInfo)
    {
        (Id, Uri, IsActive) = peerInfo;
    }

    public void Update(PeerInfo peerInfo)
    {
        if (peerInfo.PeerId != Id)
        {
            throw new ArgumentException("Id of peer info must be same to peer satte id", nameof(peerInfo));
        }

        Uri = peerInfo.PeerUri;
        IsActive = peerInfo.IsActive;
    }

    public void SetLastIndex(LogIndex logIndex)
    {
        if (logIndex < LastIndex)
        {
            throw new ArgumentException();
        }

        LastIndex = logIndex;
    }
}
