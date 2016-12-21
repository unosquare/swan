using NUnit.Framework;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ObjectStringifierTest
    {
        [Test]
        public void FromObjectTest()
        {
            var stringData = ObjectStringifier.FromObject(BasicJson.GetDefault());

            Assert.IsNotNull(stringData);
            Assert.AreEqual("Unosquare.Swan.Test.Mocks.BasicJson", stringData.ToString());
        }
    }
}
