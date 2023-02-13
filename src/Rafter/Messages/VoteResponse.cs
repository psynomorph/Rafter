using Rafter.Values;

namespace Rafter.Messages;

public sealed record VoteResponse(
    Term Term,
    PeerId PeerId,
    bool VoteGranted,
    LogIndex LogIndex,
    PeerId? CurrentLeaderId) : IRaftMessage;