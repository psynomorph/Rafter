using Rafter.Values;

namespace Rafter.Messages;

public sealed record VoteRequest(
    Term Term,
    PeerId PeerId,
    LogMeta LogMeta) 
    : IRaftMessage;
