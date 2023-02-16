using Rafter.Impl;
using Rafter.UnitTests.Mocks;
using Rafter.Values;

namespace Rafter.UnitTests;

[TestFixture]
public class RaftSmStateTests
{
    [Test]
    public void Create_state_success()
    {
        // Arrange && Act
        var state = new RaftState(
            new MockTimeProvider(),
            new PeerId(10),
            new Term(2),
            PeerRole.Candidate);

        // Assert
        state.CurrentPeerId.Should().Be(new PeerId(10));
        state.CurrentTerm.Should().Be(new Term(2));
        state.CurrentRole.Should().Be(PeerRole.Candidate);
        state.CurrentLeaderId.Should().BeNull();
        state.VotedFor.Should().BeNull();
    }

    [Test]
    public void Vote_for_candidate_success()
    {
        // Arrange
        var state = CreateState();
        var peerId = new PeerId(12);

        // Act
        state.VoteFor(peerId);

        // Assert
        state.VotedFor.Should().Be(peerId);
        state.CurrentLeaderId.Should().Be(peerId);
    }

    [Test]
    public void Reset_elections_success()
    {
        // Arrange
        var state = CreateState();
        state.VoteFor(new PeerId(12));

        // Act
        state.ResetElections();

        // Assert
        state.VotedFor.Should().BeNull();
    }

    [Test]
    public void Inc_term_success()
    {
        // Arrange
        var state = CreateState(11);

        // Act
        state.IncTerm();

        // Assert
        state.CurrentTerm.Should().Be(12);
    }

    [Test]
    public void Become_follower_success()
    {
        // Arrange
        var state = CreateState(peerRole: PeerRole.Leader);

        // Act
        state.BecomeFollower(new Term(13));

        // Assert
        state.CurrentTerm.Should().Be(13);
        state.CurrentLeaderId.Should().BeNull();
        state.CurrentRole.Should().Be(PeerRole.Follower);
    }

    [Test]
    public void Become_leader_success()
    {
        // Arrange
        var state = CreateState(peerRole: PeerRole.Candidate);

        // Act
        state.BecomeLeader();

        // Assert
        state.CurrentLeaderId.Should().Be(state.CurrentPeerId);
        state.CurrentRole.Should().Be(PeerRole.Leader);
    }

    [Test]
    public void Loose_election_success()
    {
        // Arrange
        var state = CreateState(peerRole: PeerRole.Candidate);

        // Act
        state.LooseElection();

        // Assert
        state.VotedFor.Should().BeNull();
        state.CurrentLeaderId.Should().BeNull();
        state.CurrentRole.Should().Be(PeerRole.Follower);
    }

    [Test]
    public void Grant_vote_success()
    {
        // Arrange
        var state = CreateState(peerRole: PeerRole.Follower);
        var term = new Term(14);
        var peerId = new PeerId(25);

        // Act
        state.GrantVote(term, peerId);

        // Assert
        state.CurrentTerm.Should().Be(term);
        state.CurrentLeaderId.Should().Be(peerId);
        state.VotedFor.Should().Be(peerId);
        state.CurrentRole.Should().Be(PeerRole.Follower);
    }

    [Theory]
    [TestCase(11, nameof(PeerRole.Follower), 22)]
    [TestCase(12, nameof(PeerRole.Leader), null)]
    [TestCase(20, nameof(PeerRole.Candidate), 11)]
    public void Take_snapshot_success(long termValue, string roleName, long? votedForValue)
    {
        // Arrange
        var term = new Term(termValue);
        var role = Enum.Parse<PeerRole>(roleName);
        PeerId? votedFor = votedForValue.HasValue 
            ? new PeerId(votedForValue.Value) 
            : null;

        var state = CreateState(term: term, peerRole: role);
        if (votedFor.HasValue)
        {
            state.VoteFor(votedFor.Value);
        }

        // Act
        var snapshot = state.TakeStateSnapshot();

        // Assert
        snapshot.CurrentTerm.Should().Be(term);
        snapshot.Role.Should().Be(role);
        snapshot.VotedFor.Should().Be(votedFor);
    }

    [Theory]
    [TestCase(11, nameof(PeerRole.Follower))]
    [TestCase(12, nameof(PeerRole.Leader))]
    public void Checks_current_state_success(long termValue, string roleName)
    {
        // Arrange
        var term = new Term(termValue);
        var role = Enum.Parse<PeerRole>(roleName);
        var state = CreateState(term: term, peerRole: role);

        // Act
        var matched = state.CheckCurrentState(term, role);
        var nonMatched1 = state.CheckCurrentState(term.Next(), role);
        var nonMatched2 = state.CheckCurrentState(term, PeerRole.Candidate);

        // Assert
        matched.Should().BeTrue();
        nonMatched1.Should().BeFalse();
        nonMatched2.Should().BeFalse();
    }

    [Theory]
    [TestCase(11, nameof(PeerRole.Follower))]
    [TestCase(12, nameof(PeerRole.Leader))]
    public void Checks_state_snapshot_success(long termValue, string roleName)
    {
        // Arrange
        var term = new Term(termValue);
        var role = Enum.Parse<PeerRole>(roleName);
        var state = CreateState(term: term, peerRole: role);

        // Act
        var matched = state.CheckCurrentState(new RaftState.Snapshot(term, role, null));
        var nonMatched1 = state.CheckCurrentState(new RaftState.Snapshot(term.Next(), role, null));
        var nonMatched2 = state.CheckCurrentState(new RaftState.Snapshot(term, PeerRole.Candidate, null));

        // Assert
        matched.Should().BeTrue();
        nonMatched1.Should().BeFalse();
        nonMatched2.Should().BeFalse();
    }

    [Test]
    public void Update_last_heartbeat_time_success()
    {
        // Arrange
        var timeProvider = new MockTimeProvider();
        var state = new RaftState(
            timeProvider,
            new PeerId(10),
            new Term(10),
            PeerRole.Follower);

        timeProvider.SetTimeSeq(TimeSpan.FromSeconds(12));

        // Act
        
        state.UpdateHeartBeat();

        // Assert
        state.LastHeartBeat.Should().Be(TimeSpan.FromSeconds(12));
    }

    [Test]
    public void Become_candidate_success()
    {
        // Arrange
        var state = CreateState(peerRole: PeerRole.Follower);

        // Act
        state.BecomeCandidate();

        // Assert
        state.CurrentRole.Should().Be(PeerRole.Candidate);
    }

    private static RaftState CreateState(
        Term? term = null,
        PeerRole peerRole = PeerRole.Candidate)
    {
        return new RaftState(
            new MockTimeProvider(),
            new PeerId(10),
            term ?? new Term(10),
            peerRole);
    }
}