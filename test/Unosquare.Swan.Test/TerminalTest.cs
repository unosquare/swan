using NUnit.Framework;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class TerminalTest
    {
        [Test]
        public void IsConsolePresentTest()
        {
            if (CurrentApp.OS == Os.Windows)
            {
                // Funny, the console is not here :P
                Assert.IsFalse(Terminal.IsConsolePresent); 
            }
            else
            {
                Assert.IsTrue(Terminal.IsConsolePresent);
            }
        }
    }
}
