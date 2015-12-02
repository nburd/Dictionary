using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dictionary
{
    public class myDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private IEqualityComparer<TKey> Comparer;
        private Node head;
        private int count;
        private Node[] baskets;
        private int[] mas;
        private int thresholdValue;

        public myDictionary()
        {
            baskets = new Node[7];
            for (int i = 0; i < baskets.Length; i++)
                baskets[i] = head;
            mas = new int[7];
            count = 0;
            thresholdValue = 5;
        }

        public myDictionary(IEqualityComparer<TKey> сomparer)
        {
            baskets = new Node[7];
            for (int i = 0; i < baskets.Length; i++)
                baskets[i] = head;
            mas = new int[7];
            count = 0;
            thresholdValue = 5;
            Comparer = сomparer;
        }

        private int GetIndexBaskets(int hash)
        {
            return Math.Abs(hash % baskets.Length);
        }

        private void InsertKVP(int hash, TKey key, TValue value)
        {
            var index = GetIndexBaskets(hash);
            var node = GetNode(hash, index);
            if (node == null)
            {
                node = new Node(hash, baskets[index]);
                baskets[index] = node;
            }

            if (node.KeyValuePair.Any(x => Comparer.Equals(x.Key, key)))
                throw new ArgumentException("Such key already exist");

            node.KeyValuePair.Add(new KeyValuePair(key, value));
            ++count;
            ++mas[index];
        }

        private void Add(int hash, TKey key, TValue value)
        {
            InsertKVP(hash, key, value);
            TryResize();
        }

        public void Set(int hash, TKey key, TValue newValue)
        {
            var index = GetIndexBaskets(hash);
            var current = GetNode(hash, index);

            if (current == null)
                throw new ArgumentException("Such key doesn't exist");

            current.KeyValuePair.First(x => Comparer.Equals(x.Key, key)).Value = newValue;
        }

        private bool TryGet(int hash, TKey key, out TValue value)
        {
            value = default(TValue);

            var index = GetIndexBaskets(hash);
            var current = GetNode(hash, index);
            if (current == null)
                return false;

            var kvp = current.KeyValuePair.FirstOrDefault(x => Comparer.Equals(x.Key, key));
            if (kvp == null)
                return false;

            value = kvp.Value;
            return true;
        }

        private bool Delete(int hash, TKey key)
        {
            var index = GetIndexBaskets(hash);
            var current = GetNode(hash, index);
            if (current == null)
                throw new ArgumentException("Such key doesn't exist");

            var kvp = current.KeyValuePair.FirstOrDefault(x => Comparer.Equals(x.Key, key));
            if (kvp == null)
                throw new ArgumentException("Such key doesn't exist");

            current.KeyValuePair.Remove(kvp);
            --count;
            --mas[index];
            return true;
        }

        private void TryResize()
        {
            int empty = 0;
            for (int cur = 0; cur < mas.Length; cur++)
                if (mas[cur] > thresholdValue)
                {
                    Node[] temp = (Node[])baskets.Clone();
                    baskets = new Node[baskets.Length * 2];
                    mas = new int[mas.Length * 2];
                    Copy(temp);
                    break;
                }
                else if (mas[cur] == 0)
                    empty++;
            if (empty > mas.Length / 4 && count > mas.Length)
            {
                Node[] temp = (Node[])baskets.Clone();
                baskets = new Node[baskets.Length / 2];
                mas = new int[mas.Length / 2];
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
                        var collect = current.KeyValuePair.Take(current.KeyValuePair.Count);
                        foreach (var c in collect)
                            InsertKVP(Comparer.GetHashCode(c.Key), c.Key, c.Value);
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

        #region IDictionary

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (!TryGet(Comparer.GetHashCode(key), key, out value))
                    throw new ArgumentException("Such value doesn't exist");
                return value;
            }

            set
            {
                Set(Comparer.GetHashCode(key), key, value);
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
                var current = head;
                while (current != null)
                {
                    foreach (var kvp in current.KeyValuePair)
                        Keys.Add(kvp.Key);
                    current = current.Next;
                }
                return Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                var current = head;
                while (current != null)
                {
                    foreach (var kvp in current.KeyValuePair)
                        Values.Add(kvp.Value);
                    current = current.Next;
                }
                return Values;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(Comparer.GetHashCode(item.Key), item.Key, item.Value);
        }

        public void Add(TKey key, TValue value)
        {
            Add(Comparer.GetHashCode(key), key, value);
        }

        public void Clear()
        {
            var current = head;
            while (current != null)
            {
                current.KeyValuePair.Clear();
                current = current.Next;
            }
            count = 0;
            head = null;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var index = GetIndexBaskets(Comparer.GetHashCode(item.Key));
            var current = GetNode(Comparer.GetHashCode(item.Key), index);
            return current.KeyValuePair.Any(x => (Comparer.Equals(x.Key, item.Key) && Equals(x.Value, item.Value)));
        }

        public bool ContainsKey(TKey key)
        {
            var index = GetIndexBaskets(Comparer.GetHashCode(key));
            var current = GetNode(Comparer.GetHashCode(key), index);
            return current.KeyValuePair.Any(x => Comparer.Equals(x.Key, key));
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {

        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            var current = head;
            while (current != null)
            {
                foreach (var kvp in current.KeyValuePair)
                    yield return new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value);
                current = current.Next;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Delete(Comparer.GetHashCode(item.Key), item.Key);
        }

        public bool Remove(TKey key)
        {
            return Delete(Comparer.GetHashCode(key), key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return TryGet(Comparer.GetHashCode(key), key, out value);
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

            public ICollection<KeyValuePair> KeyValuePair { get; set; }

            public Node(int hash, Node next)
            {
                Hash = hash;
                Next = next;
                KeyValuePair = new List<KeyValuePair>();
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
