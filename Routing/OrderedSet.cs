﻿using System.Collections;
using System.Collections.Generic;

namespace Routing
{
    /// <summary>
    /// <a href="https://stackoverflow.com/a/17853085/5903309">Source</a>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    internal sealed class OrderedSet<T> : ICollection<T>
    {
        private readonly IDictionary<T, LinkedListNode<T>> _dictionary;

        private readonly LinkedList<T> _linkedList = new LinkedList<T>();

        public OrderedSet()
            : this(EqualityComparer<T>.Default)
        {
        }

        public OrderedSet(IEqualityComparer<T> comparer)
        {
            _dictionary = new Dictionary<T, LinkedListNode<T>>(comparer);
        }

        public int Count => _dictionary.Count;

        public bool IsReadOnly => _dictionary.IsReadOnly;

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public bool Add(T item)
        {
            if (_dictionary.ContainsKey(item))
            {
                return false;
            }

            var node = _linkedList.AddLast(item);
            _dictionary.Add(item, node);
            return true;
        }

        public void Clear()
        {
            _linkedList.Clear();
            _dictionary.Clear();
        }

        public bool Remove(T item)
        {
            var found = _dictionary.TryGetValue(item, out var node);
            if (!found)
            {
                return false;
            }

            _dictionary.Remove(item);
            _linkedList.Remove(node);
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _linkedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(T item)
        {
            return _dictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _linkedList.CopyTo(array, arrayIndex);
        }
    }
}
