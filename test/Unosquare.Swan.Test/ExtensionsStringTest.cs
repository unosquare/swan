using NUnit.Framework;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ExtensionsStringTest
    {
        [TestCase("Camel Case", "CamelCase")]
        [TestCase("Snake Case", "Snake_Case")]
        public void HumanizeTest(string expected, string input)
        {
            Assert.AreEqual(expected, input.Humanize(), $"Testing with {input}");
        }

        [Test]
        public void ComputeHashesTest()
        {
            const string input = "HOLA";

            Assert.AreEqual("C6-F0-09-88-43-0D-BC-8E-83-A7-BC-7A-B5-25-63-46", input.ComputeMD5().ToDashedHex(), "Get MD5");
            Assert.AreEqual("261C5AD45770CC14875C8F46EAA3ECA42568104A", input.ComputeSha1().ToUpperHex(), "Get Sha1");
            Assert.AreEqual("73C3DE4175449987EF6047F6E0BEA91C1036A8599B43113B3F990104AB294A47".ConvertHexadecimalToBytes(), input.ComputeSha256(), "Get Sha256");
            Assert.AreEqual("XPWJJ7QTeLzAdrJrO4UKZuvOw6znT2uUnaVAVyHdOUiKI49a//eTtRJQOLsd1xhMHBHEf0hE0cy7MQycdYk7ZQ==", input.ComputeSha512().ToBase64(), "Get Sha512");
        }

        [TestCase("string", "string")]
        [TestCase("(null)", null)]
        public void StringifyStringsTest(string expected, object input)
        {
            Assert.AreEqual(expected, input.Stringify(), $"Testing with {input}");
        }

        [Test]
        public void StringifyObjectTest()
        {
            var objectInfoLines = BasicJson.GetDefault().Stringify().ToLines();
            Assert.Greater(7, objectInfoLines.Length);

            Assert.AreEqual("$type           : Unosquare.Swan.Test.Mocks.BasicJson", objectInfoLines[0]);
            Assert.AreEqual("StringData      : string", objectInfoLines[1]);
            Assert.AreEqual("IntData         : 1", objectInfoLines[2]);
        }
    }
}
