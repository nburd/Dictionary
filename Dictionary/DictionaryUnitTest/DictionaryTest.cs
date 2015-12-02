using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NBurd;
using System.Linq;

namespace DictionaryUnitTest
{
    [TestClass]
    public class DictionaryTest
    {
        private Dictionary<int, string> dictionary;
        private int[] keys;
        private string[] values;

        [TestInitialize]
        public void Initialize()
        {
            dictionary = new Dictionary<int, string>();
            keys = new int[]{ 1, 2, 8 };
            values = new string[]{ "dog", "cat", "mouse" };

            for (int i = 0; i < 3; i++)
                dictionary.Add(keys[i], values[i]);
        }

        [TestMethod]
        public void IndexatorSetter()
        {
            dictionary[2] = "wolf";

            Assert.AreEqual("wolf", dictionary[2]);
        }

        #region CountTests

        [TestMethod]
        public void IncreaseCount()
        {
            Initialize();

            dictionary.Add(3, "value3");

            Assert.AreEqual(4, dictionary.Count);
        }

        [TestMethod]
        public void DecreaseCount()
        {
            Initialize();

            dictionary.Remove(2);

            Assert.AreEqual(2, dictionary.Count);
        }

        [TestMethod]
        public void NotChangeCount()
        {
            Initialize();

            dictionary[2] = "value2";

            Assert.AreEqual(3, dictionary.Count);
        }

        #endregion
        
        [TestMethod]
        public void IsReadOnly()
        {
            Assert.IsFalse(dictionary.IsReadOnly);
        }

        [TestMethod]
        public void GetCollectionOfKeys()
        {
            var result = dictionary.Keys;

            Assert.AreEqual(keys.Length, result.Count);   
            Assert.IsTrue(result.All(x => keys.Contains(x)));
            Assert.IsTrue(keys.All(x => result.Contains(x)));
        }

        [TestMethod]
        public void GetCollectionOfValues()
        {
            var result = dictionary.Values;

            Assert.AreEqual(values.Length, result.Count);
            Assert.IsTrue(result.All(x => values.Contains(x)));
            Assert.IsTrue(values.All(x => result.Contains(x)));
        }

        [TestMethod]
        public void AddKeyValuePair()
        {
            dictionary = new Dictionary<int, string>();

            var kvp = new System.Collections.Generic.KeyValuePair<int, string>(1, "value");
            dictionary.Add(kvp);

            Assert.AreEqual(1, dictionary.Count);
            Assert.AreEqual("value", dictionary[1]);
        }

        [TestMethod]
        public void AddKeyAndValue()
        {
            dictionary = new Dictionary<int, string>();
                       
            dictionary.Add(1, "value");

            Assert.AreEqual(1, dictionary.Count);
            Assert.AreEqual("value", dictionary[1]);
        }

        [TestMethod]
        public void Clear()
        {
            dictionary.Clear();

            Assert.AreEqual(0, dictionary.Count);
            Assert.AreEqual(0, dictionary.Keys.Count);
            Assert.AreEqual(0, dictionary.Values.Count);
        }

        [TestMethod]
        public void ContainsKeyValuePair()
        {            
            var kvp = new System.Collections.Generic.KeyValuePair<int, string>(1, "dog");
            var result = dictionary.Contains(kvp);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ContainsKey()
        {           
            var result = dictionary.ContainsKey(1);

            Assert.IsTrue(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CopyToException()
        {
            var array = new System.Collections.Generic.KeyValuePair<int, string>[5];

            dictionary.CopyTo(array, 4);            
        }

        [TestMethod]
        public void CopyTo()
        {
            var targetArray = new System.Collections.Generic.KeyValuePair<int, string>[5];

            var a = 2;
            var b = a + dictionary.Count - 1;
            dictionary.CopyTo(targetArray, a);

            Assert.IsTrue(dictionary.All(x => InRange(a, b, targetArray.ToList().IndexOf(x))));
        }

        [TestMethod]
        public void GetEnumerator()
        {
            foreach (var kvp in dictionary)
                Assert.AreEqual(keys.ToList().IndexOf(kvp.Key), values.ToList().IndexOf(kvp.Value));
            for (int i = 0; i < keys.Length; i++)
            {
                var kvp = new System.Collections.Generic.KeyValuePair<int, string>(keys[i], values[i]);
                Assert.IsTrue(dictionary.Contains(kvp));
            }
        }

        [TestMethod]
        public void RemoveOnKeyValuePair()
        {            
            dictionary.Remove(1);

            Assert.AreEqual(2, dictionary.Count);
        }

        [TestMethod]
        public void RemoveOnKey()
        {
            var kvp = new System.Collections.Generic.KeyValuePair<int, string>(1, "dog");
            dictionary.Remove(kvp);

            Assert.AreEqual(2, dictionary.Count);
        }

        [TestMethod]
        public void TryGetValue()
        {
            string value;
            var result = dictionary.TryGetValue(1, out value);

            Assert.IsTrue(result);
            Assert.AreEqual("dog", value);            
        }

        private bool InRange(int a, int b, int value)
        {
            return a <= value && value <= b;
        }
    }
}
