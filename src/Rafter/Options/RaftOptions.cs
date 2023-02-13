using System;

namespace Rafter.Options;

public class RaftOptions
{
    public TimeSpan ElectionRoundDuration { get; set; } = TimeSpan.FromSeconds(15);
    public TimeSpan ElectionRetryInterval { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan FollowerHeartBeatInterval { get; set; } = TimeSpan.FromSeconds(60);
    public TimeSpan LeaderHeartBeatInterval { get; set; } = TimeSpan.FromSeconds(15);
    public TimeSpan HeartBeatRequestRetryInterval { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan HeartBeatRequestTimeOutInterval { get; set; } = TimeSpan.FromSeconds(5);
}
