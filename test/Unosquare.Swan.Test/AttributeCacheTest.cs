using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unosquare.Swan.Lite.Attributes;
using Unosquare.Swan.Reflection;
using Unosquare.Swan.Test.Mocks;

public abstract class AttributeCacheTest
{
    protected static readonly PropertyTypeCache TypeCache = new PropertyTypeCache();
    protected static readonly AttributeCache AttributeCache = new AttributeCache(TypeCache);
}

[TestFixture]
public class ContrainedRetrieve : AttributeCacheTest
{
    [Test]
    public void NullMemberInfo_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            AttributeCache.Retrieve<MatchAttribute>(null as MemberInfo));
    }

    [Test]
    public void ValidMember_ReturnsProperties()
    {
        var member = typeof(RegexMock).GetProperty(nameof(RegexMock.Salute));
        var attribs = AttributeCache.Retrieve<MatchAttribute>(member);

        Assert.That(attribs.Length, Is.EqualTo(1));
    }

    [Test]
    public void PropertyWithNoMatchingAttributes_ReturnsZeroProperties()
    {
        var member = typeof(NotNullMock).GetProperty(nameof(NotNullMock.Number));
        var attribs = AttributeCache.Retrieve<MatchAttribute>(member);

        Assert.That(attribs.Length, Is.EqualTo(0));
    }

    //[Test]
    //public void WithNullFactory_ThrowsArgumentNullException()
    //{
    //    Assert.Throws<ArgumentNullException>(() =>
    //        TypeCache.Retrieve(typeof(string), null));
    //}
}

[TestFixture]
public class Retrieve : AttributeCacheTest
{
    [Test]
    public void NullMemberInfo_ThrowsArgumentNullException()
    {
        var member = typeof(RegexMock).GetProperty(nameof(RegexMock.Salute));
        Assert.Throws<ArgumentNullException>(() =>
            AttributeCache.Retrieve(null as MemberInfo, typeof(IValidator)));
    }

    [Test]
    public void NullType_ThrowsArgumentNullException()
    {
        var member = typeof(RegexMock).GetProperty(nameof(RegexMock.Salute));
        Assert.Throws<ArgumentNullException>(() =>
            AttributeCache.Retrieve(member, null));
    }

    [Test]
    public void ValidParams_ReturnsAttributes()
    {
        var member = typeof(RegexMock).GetProperty(nameof(RegexMock.Salute));
        var attribs = AttributeCache.Retrieve(member, typeof(IValidator));
        Assert.That(attribs.Length, Is.EqualTo(1));
    }

    [Test]
    public void PropertyWithNoMatchingAttributes_ReturnsZeroProperties()
    {
        var member = typeof(NotNullMock).GetProperty(nameof(NotNullMock.Number));
        var attribs = AttributeCache.Retrieve(member, typeof(IReflect));

        Assert.That(attribs.Length, Is.EqualTo(0));
    }
}
