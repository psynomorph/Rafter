using System;
using System.Collections.Generic;

namespace Rafter.Values;

/// <summary>
/// Raft log index
/// </summary>
public readonly struct LogIndex : IEquatable<LogIndex>, IComparable<LogIndex>
{
    public LogIndex(long value)
    {
        Value = value;
    }

    /// <summary>
    /// Value of index
    /// </summary>
    public long Value { get; }

    public int CompareTo(LogIndex other) => Comparer<long>.Default.Compare(Value, other.Value);
    public bool Equals(LogIndex other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is LogIndex logIndex && Equals(logIndex);

    public override int GetHashCode() => HashCode.Combine(Value);

    public override string ToString() => Value.ToString();

    /// <summary>
    /// Returns next log index
    /// </summary>
    public LogIndex Next() => new(Value + 1);

    public static LogIndex Zero => new LogIndex(0);


    public static implicit operator LogIndex(long value) => new LogIndex(value);
    public static explicit operator long(LogIndex term) => term.Value;

    public static bool operator ==(LogIndex logIndex, LogIndex other) => logIndex.Value == other.Value;
    public static bool operator !=(LogIndex logIndex, LogIndex other) => logIndex.Value != other.Value;
    public static bool operator <(LogIndex logIndex, LogIndex other) => logIndex.Value < other.Value;
    public static bool operator <=(LogIndex logIndex, LogIndex other) => logIndex.Value <= other.Value;
    public static bool operator >(LogIndex logIndex, LogIndex other) => logIndex.Value > other.Value;
    public static bool operator >=(LogIndex logIndex, LogIndex other) => logIndex.Value >= other.Value;
}
