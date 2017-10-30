﻿using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Unosquare.Swan.Test.ExtensionsDictionariesTests
{
    public abstract class ExtensionsDictionariesTest
    {
        protected Dictionary<object, object> dict = new Dictionary<object, object>();
    }

    [TestFixture]
    public class GetValueOrDefault : ExtensionsDictionariesTest
    {
        [Test]
        public void NullDictionary_ThrowsArgumentNullException()
        {
            dict = null;

            Assert.Throws<ArgumentNullException>(() =>
            {
                Extensions.GetValueOrDefault(dict, 1);
            });
        }

        [Test]
        public void DictionaryWithExistingKey_ReturnsValue()
        {
            dict = new Dictionary<object, object>
            {
                { 1, "Armando" },
                { 2, "Alexey" },
                { 3, "Alejandro" },
                { 4, "Florencia" },
                { 5, "Israel" }
            };

            Assert.AreEqual(Extensions.GetValueOrDefault(dict, 3), "Alejandro");
        }

        [Test]
        public void DictionaryWithoutExistingKey_ReturnsNull()
        {
            dict = new Dictionary<object, object>
            {
                { 1, "Armando" },
                { 2, "Alexey" },
                { 3, "Alejandro" },
                { 4, "Florencia" },
                { 5, "Israel" }
            };

            Assert.AreEqual(Extensions.GetValueOrDefault(dict, 7), null);
        }
    }

    [TestFixture]
    public class ForEach : ExtensionsDictionariesTest
    {
        [Test]
        public void NullDictionary_ThrowsArgumentNullException()
        {
            Dictionary<int, int> originalDictionary = new Dictionary<int, int>();
            originalDictionary = null;

            Assert.Throws<ArgumentNullException>(() =>
            {
                originalDictionary.ForEach((key, value) =>
                {
                    originalDictionary[key] = value + 1;
                });
            });
        }
    }
}
