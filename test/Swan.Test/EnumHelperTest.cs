namespace Swan.Test.EnumHelperTest
{
    using NUnit.Framework;
    using Swan.Test.Mocks;
    using System;
    using System.Linq;

    [TestFixture]
    public class GetItemsWithIndex
    {
        [Test]
        public void WithValidIndexEnum_ReturnsTuple()
        {
            var items = EnumHelper.GetItemsWithIndex<MyEnum>().ToArray();

            Assert.AreEqual("(0, One)", items[0].ToString());
            Assert.AreEqual("(1, Two)", items[1].ToString());
            Assert.AreEqual("(2, Three)", items[2].ToString());
        }
    }

    [TestFixture]
    public class GetItemsWithValue
    {
        [Test]
        public void WithValidValueEnum_ReturnsTuple()
        {
            var items = EnumHelper.GetItemsWithValue<MyEnum>().ToArray();

            Assert.AreEqual("(1, One)", items[0].ToString());
            Assert.AreEqual("(2, Two)", items[1].ToString());
            Assert.AreEqual("(3, Three)", items[2].ToString());
        }
    }

    [TestFixture]
    public class GetFlagValues
    {
        [TestCase(MyFlag.NoneOrZero, false, new[] { 0 })]
        [TestCase(MyFlag.One, false, new[] { 0, 1 })]
        [TestCase(MyFlag.Two, false, new[] { 0, 2 })]
        [TestCase(MyFlag.All, false, new[] { 0, 1, 2, 3 })]
        [TestCase(MyFlag.One | MyFlag.Two, false, new[] { 0, 1, 2, 3 })]
        [TestCase(MyFlag.One, true, new[] { 1 })]
        [TestCase(MyFlag.Two, true, new[] { 2 })]
        [TestCase(MyFlag.All, true, new[] { 1, 2, 3 })]
        [TestCase(MyFlag.One | MyFlag.Two, true, new[] { 1, 2, 3 })]
        public void WithFlag_ReturnsListOfInt(MyFlag val, bool ignoreZero, int[] expected)
        {
            Assert.AreEqual(expected, EnumHelper.GetFlagValues<MyFlag>((int)val, ignoreZero));
        }

        [TestCase(MyFlagByte.NoneOrZero, false, new byte[] { 0 })]
        [TestCase(MyFlagByte.One, false, new byte[] { 0, 1 })]
        [TestCase(MyFlagByte.Two, false, new byte[] { 0, 2 })]
        [TestCase(MyFlagByte.All, false, new byte[] { 0, 1, 2, 3 })]
        [TestCase(MyFlagByte.One | MyFlagByte.Two, false, new byte[] { 0, 1, 2, 3 })]
        [TestCase(MyFlagByte.One, true, new byte[] { 1 })]
        [TestCase(MyFlagByte.Two, true, new byte[] { 2 })]
        [TestCase(MyFlagByte.All, true, new byte[] { 1, 2, 3 })]
        [TestCase(MyFlagByte.One | MyFlagByte.Two, true, new byte[] { 1, 2, 3 })]
        public void WithFlag_ReturnsListOfByte(MyFlagByte val, bool ignoreZero, byte[] expected)
        {
            Assert.AreEqual(expected, EnumHelper.GetFlagValues<MyFlagByte>((byte)val, ignoreZero));
        }

        [TestCase(MyFlagLong.NoneOrZero, false, new long[] { 0 })]
        [TestCase(MyFlagLong.One, false, new long[] { 0, 1 })]
        [TestCase(MyFlagLong.Two, false, new long[] { 0, 2 })]
        [TestCase(MyFlagLong.All, false, new long[] { 0, 1, 2, 3 })]
        [TestCase(MyFlagLong.One | MyFlagLong.Two, false, new long[] { 0, 1, 2, 3 })]
        [TestCase(MyFlagLong.One, true, new long[] { 1 })]
        [TestCase(MyFlagLong.Two, true, new long[] { 2 })]
        [TestCase(MyFlagLong.All, true, new long[] { 1, 2, 3 })]
        [TestCase(MyFlagLong.One | MyFlagLong.Two, true, new long[] { 1, 2, 3 })]
        public void WithFlag_ReturnsListOfLong(MyFlagLong val, bool ignoreZero, long[] expected)
        {
            Assert.AreEqual(expected, EnumHelper.GetFlagValues<MyFlagLong>((long)val, ignoreZero));
        }

        [TestCase(MyFlag2.None, false, new[] { 0 })]
        [TestCase(MyFlag2.One, false, new[] { 0, 1 })]
        [TestCase(MyFlag2.Two, false, new[] { 0, 2 })]
        [TestCase(MyFlag2.One | MyFlag2.Two, false, new[] { 0, 1, 2 })]
        [TestCase(MyFlag2.One, true, new[] { 1 })]
        [TestCase(MyFlag2.Two, true, new[] { 2 })]
        [TestCase(MyFlag2.One | MyFlag2.Two, true, new[] { 1, 2 })]
        public void WithFlag2_ReturnsListOfInt(MyFlag2 val, bool ignoreZero, int[] expected)
        {
            Assert.AreEqual(expected, EnumHelper.GetFlagValues<MyFlag2>((int)val, ignoreZero));
        }

        [Test]
        public void WithInvalidType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => EnumHelper.GetFlagValues<int>(0));
        }
    }

    [TestFixture]
    public class GetFlagNames
    {
        [TestCase(false, false, "NoneOrZero", "One")]
        [TestCase(false, true, "None Or Zero", "One")]
        [TestCase(true, false, "One", "Two")]
        public void WithIntFlag_ReturnsListOfStrings(bool ignoreZero, bool humanize, string zeroIndexValue, string oneIndexValue)
        {
            var names = EnumHelper.GetFlagNames<MyFlag>((int)MyFlag.All, ignoreZero, humanize).ToArray();

            Assert.AreEqual(zeroIndexValue, names[0]);
            Assert.AreEqual(oneIndexValue, names[1]);
        }

        [TestCase(false, false, "NoneOrZero", "One")]
        [TestCase(false, true, "None Or Zero", "One")]
        [TestCase(true, false, "One", "Two")]
        public void WithByteFlag_ReturnsListOfStrings(bool ignoreZero, bool humanize, string zeroIndexValue, string oneIndexValue)
        {
            var names = EnumHelper.GetFlagNames<MyFlagByte>((byte)MyFlagByte.All, ignoreZero, humanize).ToArray();

            Assert.AreEqual(zeroIndexValue, names[0]);
            Assert.AreEqual(oneIndexValue, names[1]);
        }

        [TestCase(false, false, "NoneOrZero", "One")]
        [TestCase(false, true, "None Or Zero", "One")]
        [TestCase(true, false, "One", "Two")]
        public void WithLongFlag_ReturnsListOfStrings(bool ignoreZero, bool humanize, string zeroIndexValue, string oneIndexValue)
        {
            var names = EnumHelper.GetFlagNames<MyFlagLong>((long)MyFlagLong.All, ignoreZero, humanize).ToArray();

            Assert.AreEqual(zeroIndexValue, names[0]);
            Assert.AreEqual(oneIndexValue, names[1]);
        }

        [Test]
        public void WithInvalidType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => EnumHelper.GetFlagNames<int>(0));
        }
    }
}