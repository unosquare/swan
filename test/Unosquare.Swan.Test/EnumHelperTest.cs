namespace Unosquare.Swan.Test.EnumHelperTest
{
    using NUnit.Framework;
    using System;
    using Components;
    using Mocks;

    public abstract class EnumHelperTest
    {
        public enum City
        {
            Stormwind = 1,
            Suramar,
            Orgrimmar,
            Dalaran
        }
    }

    [TestFixture]
    public class GetItemsWithIndex
    {
        [Test]
        public void WithValidEnum_ReturnsTuple()
        {
            var items = EnumHelper.GetItemsWithIndex<MyEnum>();

            Assert.AreEqual(items[0].ToString(), "(0, One)");
            Assert.AreEqual(items[1].ToString(), "(1, Two)");
            Assert.AreEqual(items[2].ToString(), "(2, Three)");
        }

        [Test]
        public void WithInvalidType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => EnumHelper.GetItemsWithIndex<string>());
        }
    }

    [TestFixture]
    public class GetItemsWithValue : EnumHelperTest
    {
        [Test]
        public void WithValidEnum_ReturnsTuple()
        {
            var items = EnumHelper.GetItemsWithValue<City>();

            Assert.AreEqual(items[0].ToString(), "(1, Stormwind)");
            Assert.AreEqual(items[1].ToString(), "(2, Suramar)");
            Assert.AreEqual(items[2].ToString(), "(3, Orgrimmar)");
            Assert.AreEqual(items[3].ToString(), "(4, Dalaran)");
        }

        [Test]
        public void WithInvalidType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => EnumHelper.GetItemsWithValue<string>());
        }
    }
}