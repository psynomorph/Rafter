using System;
using System.Collections;
using System.Collections.Generic;

namespace Rafter.Helper;

public static class ListSegment
{
    public static ListSegment<T> Create<T>(IReadOnlyList<T> list) => new ListSegment<T>(list);
}

public readonly struct ListSegment<T> : IReadOnlyList<T>
{
    private readonly IReadOnlyList<T> _list;
    private readonly int _offset;
    public int Count { get; }

    public ListSegment(IReadOnlyList<T> list)
    {
        if (list is null) throw new ArgumentNullException(nameof(list));

        _list = list;
        _offset = 0;
        Count = list.Count;
    }

    public ListSegment(IReadOnlyList<T> list, int offset)
    {
        if (list is null) throw new ArgumentNullException(nameof(list));
        if (offset < 0 || offset >= list.Count) throw new ArgumentOutOfRangeException(nameof(offset));

        _list = list;
        _offset = offset;
        Count = list.Count - offset;
    }

    public ListSegment(IReadOnlyList<T> list, int offset, int count)
    {
        if (list is null) throw new ArgumentNullException(nameof(list));
        if (offset < 0 || offset >= list.Count) throw new ArgumentOutOfRangeException(nameof(offset));
        if (count < 0 || offset + count > list.Count) throw new ArgumentOutOfRangeException(nameof(count));

        _list = list;
        _offset = offset;
        Count = count;
    }

    public T this[int index] => index < Count
        ? _list[_offset + index]
        : throw new ArgumentOutOfRangeException(nameof(index));

    public T this[Index index]
    {
        get
        {
            var offset = index.GetOffset(Count);
            return offset < Count
                ? _list[_offset + offset]
                : throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    public ListSegment<T> this[Range range]
    {
        get
        {
            var (offset, count) = range.GetOffsetAndLength(Count);
            return new ListSegment<T>(_list, offset, count);
        }
    }
        

    private ListSegment<T> Slice(int offset, int count)
    {
        return new ListSegment<T>(_list, offset, count);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(this);
    }

    private class Enumerator : IEnumerator<T>
    {
        private readonly ListSegment<T> _segment;

        private int _currentIndex;

        public Enumerator(ListSegment<T> segment)
        {
            _segment = segment;
            Reset();
        }

        public T Current { get; private set; } = default!;

        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _currentIndex++;
            if (_currentIndex < _segment.Count) 
            {
                Current = _segment[_segment._offset + _currentIndex];
                return true;
            }
            return false;
        }

        public void Reset()
        {
            _currentIndex = -1;
        }

        public void Dispose()
        {
            //
        }
    }
}
