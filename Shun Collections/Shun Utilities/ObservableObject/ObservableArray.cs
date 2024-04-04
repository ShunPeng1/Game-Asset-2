using System;
using System.Collections.Generic;
using System.Linq;

namespace ShunCollection.Shun_Utilities.ObservableObject
{
    public interface IObservableArray<T> 
    {
        event Action<T[]> AnyValueChanged;

        int Count { get; }
        T this[int index] { get; }
        
        void Swap(int index1, int index2);
        void Clear();
        bool TryAdd(T item);
        bool TryAddAt(int index, T item);
        bool TryRemove(T item);
        bool TryRemoveAt(int index);
    }

    [Serializable]
    public class ObservableArray<T> : IObservableArray<T> 
    {
        private T[] _items;
        public T[] Items => _items;

        public event Action<T[]> AnyValueChanged = delegate { };
        public int Count => _items.Count(i => i != null);
        public int Length => _items.Length;
        public T this[int index] => _items[index];

        public ObservableArray(int size = 20, IList<T> initialList = null) 
        {
            _items = new T[size];
            if (initialList != null) {
                initialList.Take(size).ToArray().CopyTo(_items, 0);
                Invoke();
            }
        }

        void Invoke() => AnyValueChanged.Invoke(_items);

        public void Swap(int index1, int index2) 
        {
            (_items[index1], _items[index2]) = (_items[index2], _items[index1]);
            Invoke();
        }

        public void Clear() 
        {
            _items = new T[_items.Length];
            Invoke();
        }

        public bool TryAdd(T item)
        {
            for (var i = 0; i < _items.Length; i++) {
                if (TryAddAt(i, item)) return true;
            }
            return false;
        }
        
        public bool TryAddAt(int index, T item) 
        {
            if (index < 0 || index >= _items.Length) return false;
            
            if (_items[index] != null) return false;

            _items[index] = item;
            Invoke();
            return true;
        }

        public bool TryRemove(T item) 
        {
            for (var i = 0; i < _items.Length; i++) {
                if (EqualityComparer<T>.Default.Equals(_items[i], item) && TryRemoveAt(i)) return true;
            }
            return false;
        }
        
        public bool TryRemoveAt(int index) 
        {
            if (index < 0 || index >= _items.Length) return false;
            
            if (_items[index] == null) return false;

            _items[index] = default;
            Invoke();
            return true;
        }
    }
}