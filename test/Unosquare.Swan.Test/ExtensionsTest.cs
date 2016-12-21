using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    }
}
