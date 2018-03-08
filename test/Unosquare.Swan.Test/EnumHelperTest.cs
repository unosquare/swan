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

    [TestFixture]
    public class GetFlagValues
    {
        [TestCase(MyFlag.None, false, new[] {0})]
        [TestCase(MyFlag.One, false, new[] {0, 1})]
        [TestCase(MyFlag.Two, false, new[] {0, 2})]
        [TestCase(MyFlag.All, false, new[] {0, 1, 2, 3})]
        [TestCase(MyFlag.One | MyFlag.Two, false, new[] {0, 1, 2, 3})]
        [TestCase(MyFlag.One, true, new[] {1})]
        [TestCase(MyFlag.Two, true, new[] {2})]
        [TestCase(MyFlag.All, true, new[] {1, 2, 3})]
        [TestCase(MyFlag.One | MyFlag.Two, true, new[] {1, 2, 3})]
        public void WithFlag_ReturnsListofInt(MyFlag val, bool ignoreZero, int[] expected)
        {
            Assert.AreEqual(expected, EnumHelper.GetFlagValues<MyFlag>((int) val, ignoreZero));
        }

        [TestCase(MyFlag2.None, false, new[] {0})]
        [TestCase(MyFlag2.One, false, new[] {0, 1})]
        [TestCase(MyFlag2.Two, false, new[] {0, 2})]
        [TestCase(MyFlag2.One | MyFlag2.Two, false, new[] {0, 1, 2})]
        [TestCase(MyFlag2.One, true, new[] {1})]
        [TestCase(MyFlag2.Two, true, new[] {2})]
        [TestCase(MyFlag2.One | MyFlag2.Two, true, new[] {1, 2})]
        public void WithFlag2_ReturnsListofInt(MyFlag2 val, bool ignoreZero, int[] expected)
        {
            Assert.AreEqual(expected, EnumHelper.GetFlagValues<MyFlag2>((int) val, ignoreZero));
        }
        
        [Test]
        public void WithInvalidType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => EnumHelper.GetFlagValues<int>(0));
        }
    }
}