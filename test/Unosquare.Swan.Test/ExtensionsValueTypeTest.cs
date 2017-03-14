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
    }
}
