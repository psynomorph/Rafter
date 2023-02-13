using System;
using System.Collections.Generic;

namespace Rafter.Values;

public readonly struct PeerId : IEquatable<PeerId>, IComparable<PeerId>
{
    public PeerId(long value)
    {
        Value = value;
    }

    public long Value { get; }

    public int CompareTo(PeerId other) => Comparer<long>.Default.Compare(Value, other.Value);
    public bool Equals(PeerId other) => Value == other.Value;

    public override bool Equals(object? obj) => obj is PeerId logIndex && Equals(logIndex);

    public override int GetHashCode() => HashCode.Combine(Value);

    public override string ToString() => Value.ToString();


    public static implicit operator PeerId(long value) => new PeerId(value);
    public static explicit operator long(PeerId term) => term.Value;

    public static bool operator ==(PeerId peerId, PeerId other) => peerId.Value == other.Value;
    public static bool operator !=(PeerId peerId, PeerId other) => peerId.Value != other.Value;
    public static bool operator <(PeerId peerId, PeerId other) => peerId.Value < other.Value;
    public static bool operator <=(PeerId peerId, PeerId other) => peerId.Value <= other.Value;
    public static bool operator >(PeerId peerId, PeerId other) => peerId.Value > other.Value;
    public static bool operator >=(PeerId peerId, PeerId other) => peerId.Value >= other.Value;
}
