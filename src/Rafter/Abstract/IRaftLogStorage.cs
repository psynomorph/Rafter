using Rafter.Values;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rafter.Abstract;

public interface IRaftLogStorage
{
    Task<LogMeta> GetLastEntryMetaAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<LogEntry>> GetEntriesAsync(LogIndex from, CancellationToken cancellationToken);
    Task AppendAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken);
    Task<LogEntry> GetEntryByIndex(LogIndex index, CancellationToken cancellationToken);
    Task<LogEntry> AddNewEntry(Span<byte> payload, CancellationToken cancellationToken);
}
