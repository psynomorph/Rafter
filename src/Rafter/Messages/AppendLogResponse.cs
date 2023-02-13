using Rafter.Values;

namespace Rafter.Messages;

public sealed record AppendLogResponse(
    Term Term,
    PeerId PeerId,
    bool Success,
    LogIndex MatchIndex,
    PeerId? CurrentLeaderId = null) : IRaftMessage;