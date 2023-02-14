using System;

namespace Rafter.Impl;

internal interface IRaftTimeProvider
{
    TimeSpan CurrentTime { get; }
}