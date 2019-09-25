namespace Swan.Test.ExtensionsValueTypeTest
{
    using NUnit.Framework;
    using System;
    
    [TestFixture]
    public class Clamp
    {
        [Test]
        public void WithValidInt_ClampsValue()
        {
            Assert.AreEqual(3, 3.Clamp(1, 3));

            Assert.AreEqual(-1, -1.Clamp(1, 5));
        }

        [Test]
        public void WithValidDecimal_ClampsValue()
        {
            Assert.AreEqual(3m, 3m.Clamp(1m, 3m));

            Assert.AreEqual(-1m, -1m.Clamp(1m, 5m));
            
            Assert.AreEqual(2m, 1m.Clamp(2m, 5m));
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
    public class ToStruct : TestFixtureBase
    {
        [Test]
        public void WithArrayOfBytes_ReturnsStruct()
        {
            var smallArray = new Span<byte>(new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 });
            var result = smallArray.ToStruct<int>();

            Assert.AreEqual(538976288, result);
        }

        [Test]
        public void WithNullArrayOfBytes_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => NullByteSpan.ToStruct<int>(0, 0)); 
        }
    }
}
