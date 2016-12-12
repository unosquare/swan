using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class JsonTest
    {
        private object BasicObj = new BasicJson { StringData = "string", IntData = 1, BoolData = true };

        [Test]
        public void SerializeBasicTest()
        {
            var data = Json.Serialize(BasicObj);

            Assert.IsNotNull(data);
        }
    }
}
