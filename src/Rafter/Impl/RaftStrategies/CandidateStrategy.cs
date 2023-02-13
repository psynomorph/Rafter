using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rafter.Abstract;
using Rafter.Extensions;
using Rafter.Messages;
using Rafter.Options;
using Rafter.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rafter.Impl.RaftStrategies;

internal class CandidateStrategy : IRaftStrategy
{
    private readonly IRaftLogStorage _logStorage;
    private readonly IRaftTransport _raftTransport;
    private readonly PeersStorage _peersStorage;
    private readonly ILogger _logger;
    private readonly IOptions<RaftOptions> _options;
    private readonly RaftSmState _state;

    public CandidateStrategy(
        IRaftLogStorage logStorage,
        IRaftTransport raftTransport,
        PeersStorage peersStorage,
        ILogger logger,
        IOptions<RaftOptions> options,
        RaftSmState state)
    {
        _logStorage = logStorage;
        _raftTransport = raftTransport;
        _peersStorage = peersStorage;
        _logger = logger;
        _options = options;
        _state = state;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _state.ResetElections();
        _state.IncTerm();
        _state.VoteFor(_state.CurrentPeerId);

        var snapshot = _state.TakeStateSnapshot();

        var peers = _peersStorage.GetOtherActivePeers();
        var totalPeersCount = peers.Count + 1;

        var logInfo = await _logStorage.GetLastEntryMetaAsync(cancellationToken);

        var votesGranted = 1;
        var votesRejected = 0;

        using var voteCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        voteCancellationSource.CancelAfter(_options.Value.ElectionRoundDuration);

        var peersToRetry = new List<PeerState>();

        try
        {
            do
            {
                var tasks = SendVoteRequests(peers, logInfo, voteCancellationSource.Token);
                peersToRetry.Clear();

                foreach (var task in tasks.InCompletionOrder())
                {
                    var (result, peer) = await task;

                    if (!_state.CheckCurrentState(snapshot))
                    {
                        return;
                    }

                    if (!result.Success)
                    {
                        peersToRetry.Add(peer);
                        continue;
                    }

                    var voteResponse = result.Value;
                    if (voteResponse.Term > _state.CurrentTerm)
                    {
                        _state.ResetElections();
                        _state.BecomeFollower(voteResponse.Term, voteResponse.CurrentLeaderId);
                        return;
                    }

                    if (voteResponse.VoteGranted)
                    {
                        votesGranted++;
                    }
                    else
                    {
                        votesRejected++;
                    }
                }

                if (votesGranted >= _peersStorage.QuorumSize)
                {
                    WinElection(snapshot.CurrentTerm, logInfo.LastLogIndex);
                    return;
                }

                if (votesRejected >= _peersStorage.QuorumSize)
                {
                    _state.LooseElection();
                    return;
                }

                if (votesGranted + votesRejected == totalPeersCount)
                {
                    _state.ResetElections();
                    return;
                }

                peers = peersToRetry;

                await Task.Delay(_options.Value.ElectionRetryInterval, voteCancellationSource.Token);

            } while (_state.CheckCurrentState(snapshot));
        }
        catch (TaskCanceledException) when (voteCancellationSource.IsCancellationRequested)
        {
            // Do nothing
        }
        finally
        {
            voteCancellationSource.Cancel();
        }
    }

    private Task<ValueTuple<Result<VoteResponse, VoteError>, PeerState>>[] SendVoteRequests(
        IEnumerable<PeerState> peers,
        LogMeta logInfo,
        CancellationToken cancellationToken)
    {
        var request = new VoteRequest(
            _state.CurrentTerm,
            _state.CurrentPeerId,
            logInfo);

        return peers
            .Select(peer => _raftTransport.SendVoteMessageAsync(peer.PeerInfo, request, cancellationToken).Zip(peer))
            .ToArray();
    }

    private void WinElection(Term term, LogIndex lastCommitedIndex)
    {
        if (!_state.CheckCurrentState(term, PeerRole.Candidate))
        {
            return;
        }

        _state.WinElection();

        foreach (var peer in _peersStorage.GetAllPeers())
        {
            peer.SetLastIndex(lastCommitedIndex);
        }
    }
}
