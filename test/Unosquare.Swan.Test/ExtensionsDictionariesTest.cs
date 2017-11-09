namespace Unosquare.Swan.Test.ExtensionsDictionariesTests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using System.Linq;

    public abstract class ExtensionsDictionariesTest : TestFixtureBase
    {
        protected Dictionary<int, string> Dict = new Dictionary<int, string>
        {
            {1, "Armando"},
            {2, "Alexey"},
            {3, "Alejandro"},
            {4, "Florencia"},
            {5, "Israel"}
        };
    }

    [TestFixture]
    public class GetValueOrDefault : ExtensionsDictionariesTest
    {
        [Test]
        public void NullDictionary_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => NullDict.GetValueOrDefault(1));
        }

        [Test]
        public void DictionaryWithExistingKey_ReturnsValue()
        {
            Assert.AreEqual(Dict.GetValueOrDefault(3), "Alejandro");
        }

        [Test]
        public void DictionaryWithoutExistingKey_ReturnsNull()
        {
            Assert.IsNull(Dict.GetValueOrDefault(7), null);
        }
    }

    [TestFixture]
    public class ForEach : ExtensionsDictionariesTest
    {
        [Test]
        public void NullDictionary_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => NullDict.ForEach((key, value) => { }));
        }

        [Test]
        public void NotNullDictionary_DoesForEach()
        {
            var result = 0;

            Dict.ForEach((key, value) => result += key * 2);

            Assert.AreEqual(Dict.Sum(y => y.Key * 2), result);
        }
    }
}