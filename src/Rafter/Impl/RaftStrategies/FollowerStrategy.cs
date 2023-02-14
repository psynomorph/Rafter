using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rafter.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rafter.Impl.RaftStrategies;

internal sealed class FollowerStrategy : IRaftStrategy
{
    private readonly RaftSmState _state;
    private readonly IOptions<RaftOptions> _options;
    private readonly ILogger _logger;

    public FollowerStrategy(RaftSmState state, IOptions<RaftOptions> options, ILogger logger)
    {
        _state = state;
        _options = options;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var delay = _options.Value.FollowerHeartBeatInterval;

        while (_state.CurrentRole == PeerRole.Follower && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(delay, cancellationToken);

            delay = _state.LastHeartBeat + _options.Value.FollowerHeartBeatInterval - _state.CurrentTime;
            if (delay <= TimeSpan.Zero && _state.CurrentRole == PeerRole.Follower)
            {
                _state.BecomeCandidate();
                return;
            }
        }
    }
}
