
namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using System.Threading;
    using Abstractions;

    [TestFixture]
    public class ExclusiveTimerTest
    {
        [Test]
        public void WithDefaultTimer_WaitsForOneIteration()
        {
            var i = 0;

            using (var timer = new ExclusiveTimer(() => i++, 0, 100))
            {
                Thread.Sleep(100);

                Assert.GreaterOrEqual(i, 1, "First iteration");

                Thread.Sleep(100);

                Assert.GreaterOrEqual(i, 2, "Second iteration");
            }
        }
    }
}
