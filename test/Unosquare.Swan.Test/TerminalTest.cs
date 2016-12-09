using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class TerminalTest
    {
        [Test]
        public void IsConsolePresentTest()
        {
            // Funny, the console is not here :P
            Assert.IsFalse(Terminal.IsConsolePresent);
        }
    }
}
