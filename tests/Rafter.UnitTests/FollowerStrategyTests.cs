using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Rafter.Impl;
using Rafter.Impl.RaftStrategies;
using Rafter.Options;
using Rafter.UnitTests.Mocks;

namespace Rafter.UnitTests;

[TestFixture]
public class FollowerStrategyTests
{
    [Test]
    public async Task Becomes_candidate_after_timeout()
    {
        // Arrange
        var (timeProvider, state, strategy) = CreateStrategy(TimeSpan.FromMilliseconds(10));
        timeProvider.SetTimeSeq(TimeSpan.FromMilliseconds(11), TimeSpan.FromMilliseconds(23));

        // Act
        await strategy.RunAsync(CancellationToken.None);

        // Assert
        state.CurrentRole.Should().Be(PeerRole.Candidate);
    }

    [Test]
    public async Task Resets_after_heartbeat()
    {
        // Arrange
        var (timeProvider, state, strategy) = CreateStrategy(TimeSpan.FromMilliseconds(10));
        timeProvider.SetTimeSeq(TimeSpan.FromMilliseconds(11), TimeSpan.FromMilliseconds(15), TimeSpan.FromMilliseconds(35));

        // Act
        var task = strategy.RunAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromMilliseconds(5));
        state.UpdateHeartBeat();
        await task;

        // Assert
        state.CurrentRole.Should().Be(PeerRole.Candidate);
    }

    [Test]
    public async Task Cancel_heart_beat_on_role_change()
    {
        // Arrange
        var (timeProvider, state, strategy) = CreateStrategy(TimeSpan.FromMilliseconds(10));
        timeProvider.SetTimeSeq(TimeSpan.FromMilliseconds(11), TimeSpan.FromMilliseconds(15), TimeSpan.FromMilliseconds(35));

        // Act
        var task = strategy.RunAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromMilliseconds(5));
        state.BecomeLeader();
        await task;

        // Assert
        state.CurrentRole.Should().Be(PeerRole.Leader);
    }

    private static (MockTimeProvider, RaftSmState, FollowerStrategy) CreateStrategy(TimeSpan heartBeatTimeout)
    {
        var timeProvider = new MockTimeProvider();
        var state = new RaftSmState(timeProvider, new PeerId(1), new Term(1), PeerRole.Follower);
        var strategy = new FollowerStrategy(
            state: state,
            options: MockOptions(new RaftOptions()
            {
                FollowerHeartBeatInterval = heartBeatTimeout
            }),
            new NullLoggerFactory().CreateLogger<FollowerStrategy>());

        return (timeProvider, state, strategy);
    }

    private static IOptions<RaftOptions> MockOptions(RaftOptions options)
    {
        var mock = new Mock<IOptions<RaftOptions>>();
        mock.SetupGet(x => x.Value).Returns(options);
        return mock.Object;
    }
}
