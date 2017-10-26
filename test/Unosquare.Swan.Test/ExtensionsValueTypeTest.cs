using NUnit.Framework;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ExtensionsValueTypeTest
    {
        [Test]
        public void ClampTest()
        {
            Assert.AreEqual(3, 3.Clamp(1, 3));
            Assert.AreEqual(-1, -1.Clamp(1, 5));
        }

        [Test]
        public void IsBetween_ValidData_ReturnsTrue()
        {
            var aux = 5.IsBetween(0, 7);
            Assert.IsTrue(aux);
        }

        [Test]
        public void IsBetween_ValidData_ReturnsFalse()
        {
            var aux = 9.IsBetween(0, 7);
            Assert.IsFalse(aux);
        }

        [Test]
        public void ToStruct_ArrayOfBytes_ReturnsStruct()
        {
            var smallArray = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
            var result = smallArray.ToStruct<int>();

            Assert.AreEqual(538976288, result);
        }
    }
}
