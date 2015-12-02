using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DictionaryUnitTest
{    
    [TestClass]
    public class CustomComparerDictionaryTests
    {
        private Dictionary<int, string> dictionary;
        private int[] keys;
        private string[] values;

        [TestInitialize]
        public void Initialize()
        {
            dictionary = new Dictionary<int, string>(new CustomComparer());
            keys = new int[] { 1, 2, 8 };
            values = new string[] { "dog", "cat", "mouse" };
            for (int i = 0; i < 3; i++)
                dictionary.Add(keys[i], values[i]);
        }

        [TestMethod]
        public void AddShouldSucced()
        {
            dictionary.Add(3, "value");

            Assert.AreEqual(4, dictionary.Count);
            Assert.AreEqual("value", dictionary[3]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddShouldFail()
        {
            dictionary.Add(101, "value");           
        }

        [TestMethod]
        public void IndexatorSetterShouldSucced()
        {
            dictionary[108] = "value";

            Assert.AreEqual(3, dictionary.Count);
            Assert.AreEqual("value", dictionary[8]);
        }

        [TestMethod]
        public void IndexatorGetterShouldSucced()
        {     
            Assert.AreEqual("mouse", dictionary[708]);
        }

        [TestMethod]
        public void DeleteShouldSucced()
        {
            dictionary.Remove(202);

            Assert.AreEqual(2, dictionary.Count);
            Assert.IsFalse(dictionary.ContainsValue("cat"));
        }
    }
}
