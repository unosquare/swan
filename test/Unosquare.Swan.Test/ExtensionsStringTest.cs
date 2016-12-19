using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

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
    }
}
