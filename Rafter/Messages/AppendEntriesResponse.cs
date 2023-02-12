using Rafter.Values;

namespace Rafter.Messages;

public sealed record AppendEntriesResponse(
    Term Term,
    PeerId PeerId,
    bool Success,
    LogIndex MatchIndex,
    PeerId? CurrentLeaderId = null) : IRaftMessage;
