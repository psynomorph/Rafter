using Rafter.Impl;

namespace Rafter.UnitTests.Mocks;

public class MockTimeProvider : IRaftTimeProvider
{
    private readonly Queue<TimeSpan> _queue = new Queue<TimeSpan>();

    public TimeSpan CurrentTime => _queue.TryDequeue(out var time) ? time : TimeSpan.Zero;

    public void SetTimeSeq(params TimeSpan[] times)
    {
        foreach (var time in times)
        {
            _queue.Enqueue(time);
        }
    }
}