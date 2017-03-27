using NUnit.Framework;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ExtensionsTest
    {
        [Test]
        public void CopyPropertiesToTest()
        {
            var source = BasicJson.GetDefault();
            var destination = new BasicJson();

            source.CopyPropertiesTo(destination);

            Assert.AreEqual(source.BoolData, destination.BoolData);
            Assert.AreEqual(source.DecimalData, destination.DecimalData);
            Assert.AreEqual(source.StringData, destination.StringData);
            Assert.AreEqual(source.StringNull, destination.StringNull);
        }

        [Test]
        public void IgnoredPropertiesTest()
        {
            var source = BasicJson.GetDefault();
            var destination = new BasicJson();
            string[] ignored = { "NegativeInt", "BoolData" };
            source.CopyPropertiesTo(destination, ignored);

            Assert.AreNotEqual(source.BoolData, destination.BoolData);
            Assert.AreNotEqual(source.NegativeInt, destination.NegativeInt);
            Assert.AreEqual(source.StringData, destination.StringData);
        }

        [Test]
        public void CopyPropertiesToNewTest()
        {
            var source = BasicJson.GetDefault();
            var result = source.CopyPropertiesToNew<BasicJson>();
            
            Assert.IsNotNull(result);
            Assert.AreSame(source.GetType(), result.GetType());
        }
    }
}
