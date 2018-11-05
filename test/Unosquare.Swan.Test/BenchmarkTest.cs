namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using System;
    using Components;

    [TestFixture]
    public class BenchmarkTest
    {
        [Test]
        public void NullIdentifier_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                using (Benchmark.Start(null)) { }
            });
        }

        [Test]
        public void ValidIdentifier_DumpsResult()
        {
            using (Benchmark.Start("Benchmark")) { }
            var res = Benchmark.Dump();
            Assert.IsNotNull(res);
        }
        
        [Test]
        public void WithDifferentIdentifiers_DumpMultipleResults()
        {
            using (Benchmark.Start("Benchmark")) { }

            using (Benchmark.Start("Another benchmark")) { }

            var res = Benchmark.Dump();

            var results = res.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Length;

            Assert.AreEqual(2, results);
        }
    }
}
