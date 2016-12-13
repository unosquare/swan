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
            if (CurrentApp.OS == Os.Unix)
            {
                Assert.IsTrue(Terminal.IsConsolePresent);
            }
            else
            {
                // Funny, the console is not here :P
                Assert.IsFalse(Terminal.IsConsolePresent);
            }
        }
    }
}
