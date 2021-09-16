using NUnit.Framework;
using Swan.Extensions;
using System;

namespace Swan.Test.ExtensionsValueTypeTest
{
    [TestFixture]
    public class Clamp
    {
        [Test]
        public void WithValidInt_ClampsValue()
        {
            Assert.AreEqual(3d, 3d.Clamp(1, 3.1));

            Assert.AreEqual(1, (-1).Clamp(1, 5d));
        }

        [Test]
        public void WithValidDecimal_ClampsValue()
        {
            Assert.AreEqual(3m, 3.5m.Clamp(1, 3d));

            Assert.AreEqual(1m, (-1m).Clamp(1, 5m));

            Assert.AreEqual(-2m, (-6.144m).Clamp(-2, 2d));
        }
    }

    [TestFixture]
    public class IsBetween
    {
        [Test]
        public void WithValidParams_ReturnsTrue()
        {
            var aux = 5d.IsBetween(0d, 7m);
            Assert.IsTrue(aux);
        }

        [Test]
        public void WithInvalidParams_ReturnsFalse()
        {
            var aux = 9L.IsBetween(0U, 7L);
            Assert.IsFalse(aux);
        }
    }

    [TestFixture]
    public class ToStruct : TestFixtureBase
    {
        [Test]
        public void WithArrayOfBytes_ReturnsStruct()
        {
            var smallArray = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
            var result = smallArray.ToStruct<int>();

            Assert.AreEqual(538976288, result);
        }

        [Test]
        public void WithNullArrayOfBytes_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => NullByteArray.ToStruct<int>(0, 0));
        }
    }
}