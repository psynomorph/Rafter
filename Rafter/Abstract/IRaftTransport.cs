using Rafter.Messages;
using Rafter.Values;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rafter.Abstract;

public interface IRaftTransport
{
    Task<Result<VoteResponse, Exception>> SendVoteMessageAsync(PeerInfo peer, VoteRequest message, CancellationToken cancellationToken);
    Task<Result<AppendEntriesResponse, Exception>> SendAppendEntriesMessageAsync(PeerInfo peer, AppendEntriesResponse message, CancellationToken cancellationToken);
    Task<Result<AppendLogResponse, Exception>> SendAppendLogMessageAsync(PeerInfo peer, AppendLogRequest message, CancellationToken cancellationToken);

    IDisposable AddListener(IRaftMessageListener raftListener);
}
