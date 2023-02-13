using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Rafter.Abstract;
using Rafter.Impl;
using Rafter.Impl.RaftStrategies;
using Rafter.Messages;
using Rafter.Options;
using Rafter.UnitTests.Extensions;

namespace Rafter.UnitTests;

[TestFixture]
public class CandidateStrategyTests
{
    [Test]
    public async Task Win_election_with_full_quorum_success()
    {
        // Arrange
        var peers = new[]
        {
            new PeerInfo(1, new Uri("http://10.0.0.1"), true),
            new PeerInfo(2, new Uri("http://10.0.0.2"), true),
            new PeerInfo(3, new Uri("http://10.0.0.3"), true),
        };

        var entries = new[]
        {
            new LogEntry(new LogMeta(1, 1), 0, Array.Empty<byte>()),
            new LogEntry(new LogMeta(1, 2), 0, Array.Empty<byte>()),
            new LogEntry(new LogMeta(2, 3), 0, Array.Empty<byte>()),
        };

        var term = new Term(2);

        var transport = new TransportMock(
            (2, new VoteResponse(term.Next(), 2, true, entries[^1].Meta.LastLogIndex, 1)),
            (3, new VoteResponse(term.Next(), 3, true, entries[^1].Meta.LastLogIndex, 1)));

        var state = new RaftSmState(peers[0].PeerId, term, PeerRole.Candidate);
        var storage = new PeersStorage();
        storage.UpdatePeers(peers);
        storage.SetCurrentPeerId(peers[0].PeerId);

        var electionStrategy = new CandidateStrategy(
            logStorage: MockLogStorage(entries),
            raftTransport: transport,
            peersStorage: storage,
            logger: new NullLoggerFactory().CreateLogger<CandidateStrategy>(),
            state: state,
            options: MockOptions(new RaftOptions() { ElectionRoundDuration = TimeSpan.FromMinutes(10) }));

        // Act
        await electionStrategy.RunAsync(CancellationToken.None);

        // Assert
        state.CurrentRole.Should().Be(PeerRole.Leader);
        state.CurrentTerm.Should().Be(term.Next());
        storage.GetAllPeers().Should().AllSatisfy(peer => peer.LastIndex.Should().Be(entries[^1].Meta.LastLogIndex));
    }

    [Test]
    public async Task Win_election_with_non_full_quorum_success()
    {
        // Arrange
        var peers = new[]
        {
            new PeerInfo(1, new Uri("http://10.0.0.1"), true),
            new PeerInfo(2, new Uri("http://10.0.0.2"), true),
            new PeerInfo(3, new Uri("http://10.0.0.3"), true),
            new PeerInfo(4, new Uri("http://10.0.0.3"), true),
        };

        var entries = new[]
        {
            new LogEntry(new LogMeta(1, 1), 0, Array.Empty<byte>()),
            new LogEntry(new LogMeta(1, 2), 0, Array.Empty<byte>()),
            new LogEntry(new LogMeta(2, 3), 0, Array.Empty<byte>()),
        };

        var term = new Term(2);

        var transport = new TransportMock(
            (2, new VoteResponse(term.Next(), 2, true, entries[^1].Meta.LastLogIndex, 1)),
            (3, new VoteResponse(term.Next(), 3, true, entries[^1].Meta.LastLogIndex, 1)),
            (4, new VoteResponse(term.Next(), 3, false, entries[^1].Meta.LastLogIndex, 1)));

        var state = new RaftSmState(peers[0].PeerId, term, PeerRole.Candidate);
        var storage = new PeersStorage();
        storage.UpdatePeers(peers);
        storage.SetCurrentPeerId(peers[0].PeerId);

        var electionStrategy = new CandidateStrategy(
            logStorage: MockLogStorage(entries),
            raftTransport: transport,
            peersStorage: storage,
            logger: new NullLoggerFactory().CreateLogger<CandidateStrategy>(),
            state: state,
            options: MockOptions(new RaftOptions() { ElectionRoundDuration = TimeSpan.FromMinutes(10) }));

        // Act
        await electionStrategy.RunAsync(CancellationToken.None);

        // Assert
        state.CurrentRole.Should().Be(PeerRole.Leader);
        state.CurrentTerm.Should().Be(term.Next());
        storage.GetAllPeers().Should().AllSatisfy(peer => peer.LastIndex.Should().Be(entries[^1].Meta.LastLogIndex));
    }

    [Test]
    public async Task Win_elections_with_faulting_peers()
    {
        // Arrange
        var peers = new[]
        {
            new PeerInfo(1, new Uri("http://10.0.0.1"), true),
            new PeerInfo(2, new Uri("http://10.0.0.2"), true),
            new PeerInfo(3, new Uri("http://10.0.0.3"), true),
            new PeerInfo(4, new Uri("http://10.0.0.3"), true),
        };

        var entries = new[]
        {
            new LogEntry(new LogMeta(1, 1), 0, Array.Empty<byte>()),
            new LogEntry(new LogMeta(1, 2), 0, Array.Empty<byte>()),
            new LogEntry(new LogMeta(2, 3), 0, Array.Empty<byte>()),
        };

        var term = new Term(2);

        var transport = new TransportMock(
            (2, new VoteResponse(term.Next(), 2, true, entries[^1].Meta.LastLogIndex, 1)),
            (3, new VoteResponse(term.Next(), 3, true, entries[^1].Meta.LastLogIndex, 1)),
            (4, new VoteResponse(term.Next(), 3, false, entries[^1].Meta.LastLogIndex, 1)))
        {
            Delay = TimeSpan.FromSeconds(0.1),
            Retries =
            {
                { 2, 1 },
                { 4, 2 }
            }
        };

        var state = new RaftSmState(peers[0].PeerId, term, PeerRole.Candidate);
        var storage = new PeersStorage();
        storage.UpdatePeers(peers);
        storage.SetCurrentPeerId(peers[0].PeerId);

        var electionStrategy = new CandidateStrategy(
            logStorage: MockLogStorage(entries),
            raftTransport: transport,
            peersStorage: storage,
            logger: new NullLoggerFactory().CreateLogger<CandidateStrategy>(),
            state: state,
            options: MockOptions(new RaftOptions() 
            { 
                ElectionRoundDuration = TimeSpan.FromMinutes(10),
                ElectionRetryInterval = TimeSpan.FromSeconds(0.1)
            }));

        // Act
        await electionStrategy.RunAsync(CancellationToken.None);

        // Assert
        state.CurrentRole.Should().Be(PeerRole.Leader);
        state.CurrentTerm.Should().Be(term.Next());
        storage.GetAllPeers().Should().AllSatisfy(peer => peer.LastIndex.Should().Be(entries[^1].Meta.LastLogIndex));
    }

    [Test]
    public async Task Loose_election_success()
    {
        // Arrange
        var peers = new[]
        {
            new PeerInfo(1, new Uri("http://10.0.0.1"), true),
            new PeerInfo(2, new Uri("http://10.0.0.2"), true),
            new PeerInfo(3, new Uri("http://10.0.0.3"), true),
        };

        var entries = new[]
        {
            new LogEntry(new LogMeta(1, 1), 0, Array.Empty<byte>()),
            new LogEntry(new LogMeta(1, 2), 0, Array.Empty<byte>()),
            new LogEntry(new LogMeta(2, 3), 0, Array.Empty<byte>()),
        };

        var term = new Term(2);

        var transport = new TransportMock(
            (2, new VoteResponse(term.Next(), 2, false, entries[^1].Meta.LastLogIndex, 1)),
            (3, new VoteResponse(term.Next(), 3, false, entries[^1].Meta.LastLogIndex, 1)));

        var state = new RaftSmState(peers[0].PeerId, term, PeerRole.Candidate);
        var storage = new PeersStorage();
        storage.UpdatePeers(peers);
        storage.SetCurrentPeerId(peers[0].PeerId);

        var electionStrategy = new CandidateStrategy(
            logStorage: MockLogStorage(entries),
            raftTransport: transport,
            peersStorage: storage,
            logger: new NullLoggerFactory().CreateLogger<CandidateStrategy>(),
            state: state,
            options: MockOptions(new RaftOptions() { ElectionRoundDuration = TimeSpan.FromMinutes(10) }));

        // Act
        await electionStrategy.RunAsync(CancellationToken.None);

        // Assert
        state.CurrentRole.Should().Be(PeerRole.Follower);
        state.CurrentTerm.Should().Be(term.Next());
    }

    [Test]
    public async Task Go_to_next_round_success()
    {
        // Arrange
        var peers = new[]
        {
            new PeerInfo(1, new Uri("http://10.0.0.1"), true),
            new PeerInfo(2, new Uri("http://10.0.0.2"), true),
            new PeerInfo(3, new Uri("http://10.0.0.3"), true),
            new PeerInfo(4, new Uri("http://10.0.0.4"), true),
        };

        var entries = new[]
        {
            new LogEntry(new LogMeta(1, 1), 0, Array.Empty<byte>()),
            new LogEntry(new LogMeta(1, 2), 0, Array.Empty<byte>()),
            new LogEntry(new LogMeta(2, 3), 0, Array.Empty<byte>())
        };

        var term = new Term(2);

        var transport = new TransportMock(
            (2, new VoteResponse(term.Next(), 2, true, entries[^1].Meta.LastLogIndex, 1)),
            (3, new VoteResponse(term.Next(), 3, false, entries[^1].Meta.LastLogIndex, 2)),
            (4, new VoteResponse(term.Next(), 4, false, entries[^1].Meta.LastLogIndex, 2)));

        var state = new RaftSmState(peers[0].PeerId, term, PeerRole.Candidate);
        var storage = new PeersStorage();
        storage.UpdatePeers(peers);
        storage.SetCurrentPeerId(peers[0].PeerId);

        var electionStrategy = new CandidateStrategy(
            logStorage: MockLogStorage(entries),
            raftTransport: transport,
            peersStorage: storage,
            logger: new NullLoggerFactory().CreateLogger<CandidateStrategy>(),
            state: state,
            options: MockOptions(new RaftOptions() { ElectionRoundDuration = TimeSpan.FromMinutes(10) }));

        // Act
        await electionStrategy.RunAsync(CancellationToken.None);

        // Assert
        state.CurrentRole.Should().Be(PeerRole.Candidate);
        state.CurrentTerm.Should().Be(term.Next());
    }

    [Test]
    public async Task Cancel_elections_by_timeout_success()
    {
        // Arrange
        var peers = new[]
        {
            new PeerInfo(1, new Uri("http://10.0.0.1"), true),
            new PeerInfo(2, new Uri("http://10.0.0.2"), true),
            new PeerInfo(3, new Uri("http://10.0.0.3"), true),
            new PeerInfo(4, new Uri("http://10.0.0.4"), true),
        };

        var entries = new[]
        {
            new LogEntry(new LogMeta(1, 1), 0, Array.Empty<byte>()),
            new LogEntry(new LogMeta(1, 2), 0, Array.Empty<byte>()),
            new LogEntry(new LogMeta(2, 3), 0, Array.Empty<byte>())
        };

        var term = new Term(2);

        var transport = new TransportMock(
            (2, new VoteResponse(term.Next(), 2, true, entries[^1].Meta.LastLogIndex, 1)),
            (3, new VoteResponse(term.Next(), 3, false, entries[^1].Meta.LastLogIndex, 2)),
            (4, new VoteResponse(term.Next(), 4, false, entries[^1].Meta.LastLogIndex, 2)))
        {
# if DEBUG
            Delay = TimeSpan.FromSeconds(5),
#else
            Delay = TimeSpan.FromSeconds(0.5),
# endif
        };

        var state = new RaftSmState(peers[0].PeerId, term, PeerRole.Candidate);
        var storage = new PeersStorage();
        storage.UpdatePeers(peers);
        storage.SetCurrentPeerId(peers[0].PeerId);

        var electionStrategy = new CandidateStrategy(
            logStorage: MockLogStorage(entries),
            raftTransport: transport,
            peersStorage: storage,
            logger: new NullLoggerFactory().CreateLogger<CandidateStrategy>(),
            state: state,
            options: MockOptions(new RaftOptions() 
            { 
# if DEBUG
                ElectionRoundDuration = TimeSpan.FromSeconds(1)
#else
                ElectionRoundDuration = TimeSpan.FromSeconds(0.1)
# endif
            }));

        // Act
        await electionStrategy.RunAsync(CancellationToken.None);

        // Assert
        state.CurrentRole.Should().Be(PeerRole.Candidate);
        state.CurrentTerm.Should().Be(term.Next());
    }

    [Test]
    public async Task Becomes_follower_after_receiving_message_with_greather_term()
    {
        // Arrange
        var peers = new[]
        {
            new PeerInfo(1, new Uri("http://10.0.0.1"), true),
            new PeerInfo(2, new Uri("http://10.0.0.2"), true),
            new PeerInfo(3, new Uri("http://10.0.0.3"), true),
            new PeerInfo(4, new Uri("http://10.0.0.4"), true),
        };

        var entries = new[]
        {
            new LogEntry(new LogMeta(1, 1), 0, Array.Empty<byte>()),
            new LogEntry(new LogMeta(1, 2), 0, Array.Empty<byte>()),
            new LogEntry(new LogMeta(2, 3), 0, Array.Empty<byte>()),
        };

        var term = new Term(2);

        var transport = new TransportMock(
            (2, new VoteResponse(term.Next(), 2, true, entries[^1].Meta.LastLogIndex, 1)),
            (3, new VoteResponse(term.Next(), 2, true, entries[^1].Meta.LastLogIndex, 1)),
            (4, new VoteResponse(term.Next().Next(), 3, false, entries[^1].Meta.LastLogIndex, 1)));

        var state = new RaftSmState(peers[0].PeerId, term, PeerRole.Candidate);
        var storage = new PeersStorage();
        storage.UpdatePeers(peers);
        storage.SetCurrentPeerId(peers[0].PeerId);

        var electionStrategy = new CandidateStrategy(
            logStorage: MockLogStorage(entries),
            raftTransport: transport,
            peersStorage: storage,
            logger: new NullLoggerFactory().CreateLogger<CandidateStrategy>(),
            state: state,
            options: MockOptions(new RaftOptions() { ElectionRoundDuration = TimeSpan.FromMinutes(10) }));

        // Act
        await electionStrategy.RunAsync(CancellationToken.None);

        // Assert
        state.CurrentRole.Should().Be(PeerRole.Follower);
        state.CurrentTerm.Should().Be(term.Next().Next());
    }

    [Test]
    public async Task Cancel_ellection_after_state_changing_durung_election()
    {
        // Arrange
        var peers = new[]
        {
            new PeerInfo(1, new Uri("http://10.0.0.1"), true),
            new PeerInfo(2, new Uri("http://10.0.0.2"), true),
            new PeerInfo(3, new Uri("http://10.0.0.3"), true),
            new PeerInfo(4, new Uri("http://10.0.0.4"), true),
        };

        var entries = new[]
        {
            new LogEntry(new LogMeta(1, 1), 0, Array.Empty<byte>()),
            new LogEntry(new LogMeta(1, 2), 0, Array.Empty<byte>()),
            new LogEntry(new LogMeta(2, 3), 0, Array.Empty<byte>())
        };

        var term = new Term(2);

        var transport = new TransportMock(
            (2, new VoteResponse(term.Next(), 2, true, entries[^1].Meta.LastLogIndex, 1)),
            (3, new VoteResponse(term.Next(), 3, false, entries[^1].Meta.LastLogIndex, 2)),
            (4, new VoteResponse(term.Next(), 4, true, entries[^1].Meta.LastLogIndex, 2)))
        {
            Delay = TimeSpan.FromSeconds(0.2)
        };

        var state = new RaftSmState(peers[0].PeerId, term, PeerRole.Candidate);
        var storage = new PeersStorage();
        storage.UpdatePeers(peers);
        storage.SetCurrentPeerId(peers[0].PeerId);

        var electionStrategy = new CandidateStrategy(
            logStorage: MockLogStorage(entries),
            raftTransport: transport,
            peersStorage: storage,
            logger: new NullLoggerFactory().CreateLogger<CandidateStrategy>(),
            state: state,
            options: MockOptions(new RaftOptions()
            {
                ElectionRoundDuration = TimeSpan.FromSeconds(30)
            }));

        // Act
        var task = electionStrategy.RunAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(0.05));
        state.BecomeFollower(term.Next().Next(), 3);
        await task;

        // Assert
        state.CurrentRole.Should().Be(PeerRole.Follower);
        state.CurrentTerm.Should().Be(term.Next().Next());
    }

    private static IRaftLogStorage MockLogStorage(LogEntry[] entries)
    {
        var logStorageMock = new Mock<IRaftLogStorage>();
        logStorageMock.Setup(storage => storage.GetLastEntryMetaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                return entries.Length > 0
                    ? entries[^1].Meta
                    : new LogMeta(Term.Zero, LogIndex.Zero);
            });

        return logStorageMock.Object;
    }

    private static IOptions<RaftOptions> MockOptions(RaftOptions options)
    {
        var mock = new Mock<IOptions<RaftOptions>>();
        mock.SetupGet(x => x.Value).Returns(options);
        return mock.Object;
    }

    private class TransportMock : IRaftTransport
    {
        private readonly Dictionary<PeerId, Result<VoteResponse, VoteError>> _responses;
        private readonly Random _rand = new Random();

        public TimeSpan Delay { get; init; } = TimeSpan.Zero;
        public Dictionary<PeerId, int> Retries { get; init; } = new Dictionary<PeerId, int>();

        public TransportMock(params (PeerId, Result<VoteResponse, VoteError>)[] responses)
        {
            _responses = responses.ToDictionary(r => r.Item1, r => r.Item2);
        }

        public IDisposable AddListener(IRaftMessageListener raftListener)
        {
            throw new NotImplementedException();
        }

        public Task<Result<AppendEntriesResponse, AppendEntriesError>> SendAppendEntriesMessageAsync(PeerInfo peer, AppendEntriesRequest message, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Result<AppendLogResponse, Exception>> SendAppendLogMessageAsync(PeerInfo peer, AppendLogRequest message, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<VoteResponse, VoteError>> SendVoteMessageAsync(PeerInfo peer, VoteRequest message, CancellationToken cancellationToken)
        {
            if (Delay > TimeSpan.Zero)
            {
                await Task.Delay(Delay, cancellationToken);
            }
            else
            {
                await Task.Yield();
            }

            if (Retries.TryGetValue(peer.PeerId, out var retries) && retries > 0)
            {
                Retries[peer.PeerId] = retries - 1;
                return _rand.NextEnumValue<VoteError>();
            }

            return _responses[peer.PeerId];
        }
    }
}
