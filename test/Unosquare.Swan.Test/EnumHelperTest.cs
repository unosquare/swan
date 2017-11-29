namespace Unosquare.Swan.Test.EnumHelperTest
{
    using NUnit.Framework;
    using System;
    using Components;
    using Mocks;

    [TestFixture]
    public class GetItemsWithIndex
    {
        [Test]
        public void WithValidIndexEnum_ReturnsTuple()
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
    public class GetItemsWithValue
    {
        [Test]
        public void WithValidValueEnum_ReturnsTuple()
        {
            var items = EnumHelper.GetItemsWithValue<MyEnum>();

            Assert.AreEqual(items[0].ToString(), "(0, One)");
            Assert.AreEqual(items[1].ToString(), "(1, Two)");
            Assert.AreEqual(items[2].ToString(), "(2, Three)");
        }

        [Test]
        public void WithInvalidType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => EnumHelper.GetItemsWithValue<string>());
        }
    }
}