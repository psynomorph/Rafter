using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rafter.Abstract;
using Rafter.Helper;
using Rafter.Options;
using Rafter.Values;

namespace Rafter.Impl.RaftStrategies;

internal sealed class LeaderStrategy : IRaftStrategy
{
    private readonly IRaftTransport _transport;
    private readonly IRaftLogStorage _logStorage;
    private readonly RaftState _state;
    private readonly PeersStorage _peersStorage;
    private readonly ILogger _logger;
    private readonly IOptions<RaftOptions> _options;

    public LeaderStrategy(
        IRaftTransport transport, IRaftLogStorage logStorage, RaftState state, 
        PeersStorage peersStorage, ILogger logger, IOptions<RaftOptions> options)
    {
        _transport = transport;
        _logStorage = logStorage;
        _state = state;
        _peersStorage = peersStorage;
        _logger = logger;
        _options = options;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using var leadershipCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        try
        {
            while (_state.IsLeader && !cancellationToken.IsCancellationRequested)
            {
                var state = _state.TakeStateSnapshot();
                var startTime = _state.CurrentTime;
                await SendAppendLogAsync(state, leadershipCancellation.Token);

                var delay = startTime + _options.Value.LeaderHeartBeatInterval - _state.CurrentTime;
                await Task.Delay(delay, leadershipCancellation.Token);
            }
        }
        catch(TaskCanceledException) when (leadershipCancellation.IsCancellationRequested)
        {

        }
        finally
        {
            leadershipCancellation.Cancel();
        }
    }

    private async Task SendAppendLogAsync(RaftState.Snapshot snapshot, CancellationToken token)
    {
        var activePeers = _peersStorage.GetOtherActivePeers();
        var minLogIndex = activePeers.Min(peer => peer.LastIndex);

        var logEntries = ListSegment.Create(
            await _logStorage.GetEntriesAsync(minLogIndex, token));

        var tasks = new Task[activePeers.Count];

        var index = 0;
        foreach(var peer in activePeers)
        {
            var entriesOffset = (int)(peer.LastIndex.Value - minLogIndex.Value);
            var peerEntries = logEntries[entriesOffset..];
            var task = SendAppendLogAsync(snapshot, peer, peerEntries, token);
            tasks[index++] = task;
        }

        await Task.WhenAll(tasks);
    }

    private async Task SendAppendLogAsync(
        RaftState.Snapshot snapshot,
        PeerState state,
        ListSegment<LogEntry> entries,
        CancellationToken cancellation)
    {

    }
}
