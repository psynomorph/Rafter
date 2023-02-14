using System;
using System.Diagnostics;

namespace Rafter.Impl;

internal class RaftTimeProvider : IRaftTimeProvider
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    public TimeSpan CurrentTime => _stopwatch.Elapsed;
}