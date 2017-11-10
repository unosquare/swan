namespace Unosquare.Swan.Test.ExtensionsValueTypeTest
{
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class ExtensionsValueTypeTest
    {
        protected byte[] NullByteArray = null;
    }

    [TestFixture]
    public class Clamp
    {
        [Test]
        public void WithValidParams_ClampsValue()
        {
            Assert.AreEqual(3, 3.Clamp(1, 3));
            Assert.AreEqual(-1, -1.Clamp(1, 5));
        }
    }

    [TestFixture]
    public class IsBetween
    {
        [Test]
        public void WithValidParams_ReturnsTrue()
        {
            var aux = 5.IsBetween(0, 7);
            Assert.IsTrue(aux);
        }

        [Test]
        public void WithInvalidParams_ReturnsFalse()
        {
            var aux = 9.IsBetween(0, 7);
            Assert.IsFalse(aux);
        }
    }

    [TestFixture]
    public class ToStruct : ExtensionsValueTypeTest
    {
        [Test]
        public void WithArrayOfBytes_ReturnsStruct()
        {
            var smallArray = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
            var result = smallArray.ToStruct<int>();

            Assert.AreEqual(538976288, result);
        }

        [Test]
        public void WithNullArrayOfBytes_ReturnsStruct()
        {
            Assert.Throws<ArgumentNullException>(() => NullByteArray.ToStruct<int>(0, 0)); 
        }
    }
}
