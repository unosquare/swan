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

            Assert.AreEqual("(0, One)", items[0].ToString());
            Assert.AreEqual("(1, Two)", items[1].ToString());
            Assert.AreEqual("(2, Three)", items[2].ToString());
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

            Assert.AreEqual("(1, One)", items[0].ToString());
            Assert.AreEqual("(2, Two)", items[1].ToString());
            Assert.AreEqual("(3, Three)", items[2].ToString());
        }

        [Test]
        public void WithInvalidType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => EnumHelper.GetItemsWithValue<string>());
        }
    }
}