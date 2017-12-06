namespace Unosquare.Swan.Test.DateTimeSpanTest
{
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class CompareDates
    {
        private DateTime date = new DateTime(2002, 7, 3, 12, 0, 0, 200);

        [Test]
        public void WithFullDateTimes_ReturnsDateTimeSpan()
        {
            var result = DateTimeSpan.CompareDates(
                date,
                new DateTime(1969, 8, 15, 5, 7, 10, 100));

            Assert.AreEqual(result.Years, 32);
            Assert.AreEqual(result.Months, 10);
            Assert.AreEqual(result.Days, 18);
            Assert.AreEqual(result.Minutes, 52);
            Assert.AreEqual(result.Seconds, 50);
            Assert.AreEqual(result.Milliseconds, 100);
        }

        [Test]
        public void WithPartialDateTimes_ReturnsDateTimeSpan()
        {
            var result = DateTimeSpan.CompareDates(
                new DateTime(1969, 8, 15),
                date);

            Assert.AreEqual(result.Years, 32);
            Assert.AreEqual(result.Months, 10);
            Assert.AreEqual(result.Days, 18);
            Assert.AreEqual(result.Minutes, 0);
            Assert.AreEqual(result.Seconds, 0);
            Assert.AreEqual(result.Milliseconds, 200);
        }
    }
}