using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NBurd
{
    public class Dictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        #region Fields

        private readonly IEqualityComparer<TKey> comparer = EqualityComparer<TKey>.Default;        
        private Node[] baskets = new Node[7];
        private int[] counters = new int[7];
        private int count = 0;
        private Node head;
        private int threshold = 5;

        #endregion
        #region Constructors

        public Dictionary()
        {
            for(int i = 0; i < baskets.Length; i++)
                baskets[i] = head;
        }

        public Dictionary(IEqualityComparer<TKey> сomparer)
        {
            for (int i = 0; i < baskets.Length; i++)
                baskets[i] = head;
            this.comparer = сomparer;
        }

        #endregion
        #region Private Methods

        private int GetIndexOfBasket(int hash)
        {
            return Math.Abs(hash % baskets.Length);
        }

        private void InsertKeyValuePair(int hash, TKey key, TValue value)
        {
            var index = GetIndexOfBasket(hash);
            var node = GetNode(hash, index);
            if (node == null)
            {
                node = new Node(hash, baskets[index]);
                baskets[index] = node;
            }

            if (node.KeyValuePairCollection.Any(x => comparer.Equals(x.Key, key)))
                throw new ArgumentException("Such key already exist");

            node.KeyValuePairCollection.Add(new KeyValuePair(key, value));
            ++count;
            ++counters[index];
        }

        private void Add(int hash, TKey key, TValue value)
        {
            InsertKeyValuePair(hash, key, value);
            TryResize();
        }

        public void Set(int hash, TKey key, TValue newValue)
        {
            var index = GetIndexOfBasket(hash);
            var current = GetNode(hash, index);

            if (current == null)
                throw new ArgumentException("Such key doesn't exist");

            current.KeyValuePairCollection.First(x => comparer.Equals(x.Key, key)).Value = newValue;
        }

        private bool TryGet(int hash, TKey key, out TValue value)
        {
            value = default(TValue);

            var index = GetIndexOfBasket(hash);
            var current = GetNode(hash, index);
            if (current == null)
                return false;

            var kvp = current.KeyValuePairCollection.FirstOrDefault(x => comparer.Equals(x.Key, key));
            if (kvp == null)
                return false;

            value = kvp.Value;
            return true;
        }

        private bool Delete(int hash, TKey key)
        {
            var index = GetIndexOfBasket(hash);
            var current = GetNode(hash, index);
            if (current == null)
                throw new ArgumentException("Such key doesn't exist");

            var kvp = current.KeyValuePairCollection.FirstOrDefault(x => comparer.Equals(x.Key, key));
            if (kvp == null)
                throw new ArgumentException("Such key doesn't exist");

            current.KeyValuePairCollection.Remove(kvp);
            --count;
            --counters[index];
            return true;
        }

        private void TryResize()
        {
            int empty = 0;
            for (int cur = 0; cur < counters.Length; cur++)
                if (counters[cur] > threshold)
                {
                    Node[] temp = (Node[])baskets.Clone();
                    baskets = new Node[baskets.Length * 2];
                    counters = new int[counters.Length * 2];
                    Copy(temp);
                    break;
                }
                else if (counters[cur] == 0)
                    empty++;
            if (empty > counters.Length / 4 && count > counters.Length)
            {
                Node[] temp = (Node[])baskets.Clone();
                baskets = new Node[baskets.Length / 2];
                counters = new int[counters.Length / 2];
                Copy(temp);
            }
        }

        private void Copy(Node[] temp)
        {
            count = 0;
            for (var i = 0; i < temp.Length; i++)
                if (temp[i] != null)
                {
                    var current = temp[i];
                    while (current != null)
                    {
                        foreach (var kvp in current.KeyValuePairCollection)
                            InsertKeyValuePair(comparer.GetHashCode(kvp.Key), kvp.Key, kvp.Value);
                        current = current.Next;
                    }
                }
        }

        private Node GetNode(int hash, int index)
        {
            var current = baskets[index];
            while (current != null)
            {
                if (current.Hash == hash)
                    return current;
                current = current.Next;
            }
            return current;
        }

        #endregion
        #region IDictionary

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (!TryGet(comparer.GetHashCode(key), key, out value))
                    throw new ArgumentException("Such value doesn't exist");
                return value;
            }

            set
            {
                Set(comparer.GetHashCode(key), key, value);
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                var result = new List<TKey>();
                for (int i = 0; i < baskets.Length; i++)
                {
                    if (baskets[i] == null)
                        continue;
                                        
                    var current = baskets[i];
                    while (current != null)
                    {
                        var kvps = current.KeyValuePairCollection;
                        foreach (var kvp in kvps)
                            result.Add(kvp.Key);
                        current = current.Next;
                    }                    
                }
                return result;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                var result = new List<TValue>();
                for (int i = 0; i < baskets.Length; i++)
                {
                    if (baskets[i] != null)
                    {
                        var current = baskets[i];
                        while (current != null)
                        {
                            foreach (var cur in current.KeyValuePairCollection)
                                result.Add(cur.Value);
                            current = current.Next;
                        }
                    }
                }
                return result;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(comparer.GetHashCode(item.Key), item.Key, item.Value);
        }

        public void Add(TKey key, TValue value)
        {
            Add(comparer.GetHashCode(key), key, value);
        }

        public void Clear()
        {
            baskets = new Node[7];
            counters = new int[7];
            count = 0;
            head = null;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var index = GetIndexOfBasket(comparer.GetHashCode(item.Key));
            var current = GetNode(comparer.GetHashCode(item.Key), index);
            return current.KeyValuePairCollection
                .Any(x => (comparer.Equals(x.Key, item.Key) && Equals(x.Value, item.Value)));
        }

        public bool ContainsKey(TKey key)
        {
            var index = GetIndexOfBasket(comparer.GetHashCode(key));
            var current = GetNode(comparer.GetHashCode(key), index);
            return current.KeyValuePairCollection.Any(x => comparer.Equals(x.Key, key));
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {            
            if (array.Length > count + arrayIndex)
                throw new ArgumentException("Lead to out of range.");
            for (int i = 0; i < count; ++i, ++arrayIndex)
                foreach (var key in Keys)
                {
                    TValue value;
                    TryGetValue(key, out value);
                    array[arrayIndex] = new KeyValuePair<TKey, TValue>(key, value); 
                }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            var result = new List<TKey>();
            for (int i = 0; i < baskets.Length; i++)
            {
                if (baskets[i] == null)
                    continue;

                var current = baskets[i];
                while (current != null)
                {
                    foreach (var cur in current.KeyValuePairCollection)
                        yield return new KeyValuePair<TKey, TValue>(cur.Key, cur.Value);
                    current = current.Next;
                }                
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Delete(comparer.GetHashCode(item.Key), item.Key);
        }

        public bool Remove(TKey key)
        {
            return Delete(comparer.GetHashCode(key), key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return TryGet(comparer.GetHashCode(key), key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
        #region Nested classes

        private class Node
        {
            public Node Next { get; set; }

            public int Hash { get; set; }

            public ICollection<KeyValuePair> KeyValuePairCollection { get; set; }

            public Node(int hash, Node next)
            {
                Hash = hash;
                Next = next;
                KeyValuePairCollection = new List<KeyValuePair>();
            }
        }

        private class KeyValuePair
        {
            public TKey Key { get; set; }

            public TValue Value { get; set; }

            public KeyValuePair(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }

        #endregion
    }
}
