using Rafter.Values;
using System.Collections.Generic;

namespace Rafter.Messages;

public sealed record AppendEntriesRequest(
    Term Term,
    PeerId PeerId,
    LogMeta PrevLogMeta,
    LogIndex LeaderCommit,
    IReadOnlyCollection<LogEntry> Entries) : IRaftMessage;
        