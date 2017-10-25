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
            var aux = Extensions.IsBetween(5, 0, 7);
            Assert.IsTrue(aux);
        }
    }
}
