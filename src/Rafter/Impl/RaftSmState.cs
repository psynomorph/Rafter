using Rafter.Values;
using System;
using System.Runtime.CompilerServices;

namespace Rafter.Impl;

internal class RaftSmState
{
    /// <summary>
    /// Use stopwatch to get relative continous time
    /// </summary>
    private readonly IRaftTimeProvider _timeProvider;

    public PeerId CurrentPeerId { get; }
    public Term CurrentTerm { get; private set; } = Term.Zero;
    public PeerId? CurrentLeaderId { get; private set; }
    public PeerRole CurrentRole { get; private set; }
    public PeerId? VotedFor { get; private set; }
    public TimeSpan LastHeartBeat { get; private set; } = TimeSpan.Zero;
    public TimeSpan CurrentTime => _timeProvider.CurrentTime;

    public RaftSmState(IRaftTimeProvider timeProvider, PeerId currentPeerId, Term term, PeerRole peerRole)
    {
        _timeProvider = timeProvider;
        CurrentPeerId = currentPeerId;
        CurrentTerm = term;
        CurrentRole = peerRole;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void VoteFor(PeerId leaderId)
    {
        CurrentLeaderId = leaderId;
        VotedFor = leaderId;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetElections()
    {
        VotedFor = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IncTerm()
    {
        CurrentTerm = CurrentTerm.Next();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BecomeFollower(Term term, PeerId? leaderId = null)
    {
        CurrentTerm = term;
        CurrentLeaderId = leaderId;
        CurrentRole = PeerRole.Follower;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BecomeLeader()
    {
        CurrentLeaderId = CurrentPeerId;
        CurrentRole = PeerRole.Leader;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LooseElection()
    {
        VotedFor = null;
        CurrentLeaderId = null;
        CurrentRole = PeerRole.Follower;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GrantVote(Term term, PeerId voteFor)
    {
        CurrentTerm = term;
        CurrentLeaderId = VotedFor = voteFor;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Snapshot TakeStateSnapshot() => new(CurrentTerm, CurrentRole, VotedFor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CheckCurrentState(Snapshot snapshot)
    {
        return CurrentTerm == snapshot.CurrentTerm && CurrentRole == snapshot.Role;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CheckCurrentState(Term term, PeerRole role)
    {
        return CurrentTerm == term && CurrentRole == role;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateHeartBeat()
    {
        LastHeartBeat = CurrentTime;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BecomeCandidate()
    {
        CurrentRole = PeerRole.Candidate;
    }

    public readonly record struct Snapshot(
        Term CurrentTerm, 
        PeerRole Role,
        PeerId? VotedFor);
}
