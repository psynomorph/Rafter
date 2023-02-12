using System;

namespace Rafter.Values;

public readonly record struct PeerInfo(
    PeerId PeerId,
    Uri PeerUri,
    bool IsActive);