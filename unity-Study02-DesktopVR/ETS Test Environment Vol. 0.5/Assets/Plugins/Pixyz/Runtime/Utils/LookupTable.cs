using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pixyz.Utils  {

    public sealed class LookupTable<K, V> : IDictionary<K, HashSet<V>> {

        [NonSerialized]
        private Dictionary<K, HashSet<V>> _dictionary = new Dictionary<K, HashSet<V>>();

        public HashSet<V> this[K key] {
            get { return _dictionary[key]; }
            set { _dictionary[key] = value; }
        }

        public ICollection<K> Keys {
            get { return _dictionary.Keys; }
        }
           
        public ICollection<HashSet<V>> Values {
            get { return _dictionary.Values; }
        }

        public int Count {
            get { return _dictionary.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public void Add(K key, V value) {
            if (_dictionary.ContainsKey(key)) {
                _dictionary[key].Add(value);
            } else {
                _dictionary.Add(key, new HashSet<V> { value });
            }
        }

        public void Add(K key, HashSet<V> value) {
            _dictionary.Add(key, value);
        }

        public void Add(KeyValuePair<K, HashSet<V>> item) {
            _dictionary.Add(item.Key, item.Value);
        }

        public void Clear() {
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<K, HashSet<V>> item) {
            return _dictionary.Contains(item);
        }

        public bool ContainsKey(K key) {
            return _dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<K, HashSet<V>>[] array, int arrayIndex) {
            int i = 0;
            foreach (KeyValuePair<K, HashSet<V>> pair in this) {
                if (i >= arrayIndex) {
                    array[i] = pair;
                }
                i++;
            }
        }

        public IEnumerator<KeyValuePair<K, HashSet<V>>> GetEnumerator() {
            return _dictionary.GetEnumerator();
        }

        public bool Remove(K key) {
            return _dictionary.Remove(key);
        }

        public bool Remove(KeyValuePair<K, HashSet<V>> item) {
            return _dictionary.Remove(item.Key);
        }

        public bool TryGetValue(K key, out HashSet<V> value) {
            return _dictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
