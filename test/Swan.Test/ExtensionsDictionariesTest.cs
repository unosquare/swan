using NUnit.Framework;
using Swan.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Swan.Test.ExtensionsDictionariesTests
{
    [TestFixture]
    public class GetValueOrDefault : TestFixtureBase
    {
        [Test]
        public void NullDictionary_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => NullDict.GetValueOrDefault(1));
        }

        [Test]
        public void DictionaryWithExistingKey_ReturnsValue()
        {
            Assert.AreEqual(DefaultDictionary.GetValueOrDefault(3), "C");
        }

        [Test]
        public void DictionaryWithoutExistingKey_ReturnsNull()
        {
            Assert.IsNull(DefaultDictionary.GetValueOrDefault(7));
        }
    }

    [TestFixture]
    public class ForEach : TestFixtureBase
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

            DefaultDictionary.ForEach((key, value) => result += key * 2);

            Assert.AreEqual(DefaultDictionary.Sum(y => y.Key * 2), result);
        }
    }
}