namespace Swan.Test.ExtensionsDictionariesTests;

using NUnit.Framework;
using System;
using System.Collections.Generic;

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
