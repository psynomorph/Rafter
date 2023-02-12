using Rafter.Values;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rafter.Abstract;

public interface IRafter
{
    LogIndex LastLogIndex { get; }

    Task<LogEntry> GetEntryByIndexAsync(LogIndex logIndex, CancellationToken cancellationToken);
    Task AppendLog(LogEntry entry, CancellationToken cancellationToken);

    IDisposable AddLeadershipLeastener(IRaftLeadershipListener leadershipListener);
}
