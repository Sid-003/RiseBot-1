using System;
using System.Collections;
using System.Collections.Generic;

namespace ClashWrapper
{
    internal class ReadOnlyCollection<T> : IReadOnlyCollection<T>
    {
        private readonly IEnumerable<T> _enumerable;
        private readonly Func<int> _count;

        public int Count => _count();

        public ReadOnlyCollection(IEnumerable<T> enumerable, Func<int> count)
        {
            _enumerable = enumerable;
            _count = count;
        }

        public IEnumerator<T> GetEnumerator()
            => _enumerable.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _enumerable.GetEnumerator();

        public static ReadOnlyCollection<T> EmptyCollection()
        {
            return new ReadOnlyCollection<T>(new T[] {}, () => 0);
        }
    }
}
