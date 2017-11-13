namespace Unosquare.Swan.Test.ExtensionsDatesTests
{
    using System;
    using NUnit.Framework;
    using System.Collections.Generic;

    [TestFixture]
    public class ToSortableDate
    {
        [TestCase("2016-01-01", "00:00:00", 2016, 1, 1, 0, 0, 0)]
        [TestCase("2016-10-10", "10:10:10", 2016, 10, 10, 10, 10, 10)]
        public void ExtensionsDates_ReturnsEquals(
            string expectedDate, 
            string expectedTime, 
            int year, 
            int month,
            int day, 
            int hour,
            int minute, 
            int second)
        {
            var input = new DateTime(year, month, day, hour, minute, second);
            Assert.AreEqual(expectedDate, input.ToSortableDate());
            Assert.AreEqual($"{expectedDate} {expectedTime}", input.ToSortableDateTime());

            Assert.AreEqual(input, $"{expectedDate} {expectedTime}".ToDateTime());
        }
    }

    [TestFixture]
    public class ToDateTime
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void InvalidArguments_ThrowsArgumentNullException(string date)
        {
            Assert.Throws<ArgumentNullException>(() => date.ToDateTime());
        }

        [TestCase("2017 10 26")]
        [TestCase("2017-10")]
        [TestCase("2017-10-26 15:35")]
        public void DatesNotParsable_ThrowsException(string date)
        {
            Assert.Throws<ArgumentException>(() => date.ToDateTime());
        }
    }

    [TestFixture]
    public class DateRange
    {
        [Test]
        public void GivingTwoDates_ReturnsEqualSequenceRangeOfDates()
        {
            var startDate = new DateTime(2017, 1, 1);
            var endDate = new DateTime(2017, 1, 3);

            var rangeActual = startDate.DateRange(endDate);

            var rangeExpected = new List<DateTime>
            {
                new DateTime(2017, 1, 1, 0, 0, 0),
                new DateTime(2017, 1, 2, 0, 0, 0),
                new DateTime(2017, 1, 3, 0, 0, 0),
            };

            CollectionAssert.AreEqual(rangeExpected, rangeActual);
        }
    }

    [TestFixture]
    public class RoundUp
    {
        [Test]
        public void GivingADate_RoundUp()
        {
            var datetime = new DateTime(2017, 10, 27, 12, 35, 10);
            var timeSpan = new TimeSpan(4, 4, 10, 23);
            var expectedTicksIntoDateTime = new DateTime(636449107780000000);

            Assert.AreEqual(expectedTicksIntoDateTime, datetime.RoundUp(timeSpan));
        }
    }

    [TestFixture]
    public class ToUnixEpochDate
    {
        [Test]
        public void GivingADate_ConvertItIntoTicks()
        {
            var date = new DateTime(2017, 10, 27);

            Assert.AreEqual(1509062400, date.ToUnixEpochDate());
        }
    }
}