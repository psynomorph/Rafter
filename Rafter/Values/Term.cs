using System;
using System.Collections.Generic;

namespace Rafter.Values;

public readonly struct Term : IEquatable<Term>, IComparable<Term>
{
    public Term(long value)
    {
        Value = value;
    }

    public long Value { get; }

    public int CompareTo(Term other) => Comparer<long>.Default.Compare(Value, other.Value);
    public bool Equals(Term other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is Term term && Equals(term);
    public override int GetHashCode() => HashCode.Combine(Value);
    public override string ToString() => Value.ToString();
    public Term Next() => new(Value + 1);


    public static implicit operator Term(long value) => new Term(value);
    public static explicit operator long(Term term) => term.Value;

    public static bool operator ==(Term term, Term other) => term.Value == other.Value;
    public static bool operator !=(Term term, Term other) => term.Value != other.Value;
    public static bool operator <(Term term, Term other) => term.Value < other.Value;
    public static bool operator <=(Term term, Term other) => term.Value <= other.Value;
    public static bool operator >(Term term, Term other) => term.Value > other.Value;
    public static bool operator >=(Term term, Term other) => term.Value >= other.Value;
}
