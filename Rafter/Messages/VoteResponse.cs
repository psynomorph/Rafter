using Rafter.Values;

namespace Rafter.Messages;

public sealed record VoteResponse(
    Term Term,
    PeerId PeerId,
    bool VoteGranted,
    PeerId? CurrentLeaderId = null) : IRaftMessage;