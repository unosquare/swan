using System;
using NUnit.Framework;
using System.Collections.Generic;

namespace Unosquare.Swan.Test.ExtensionsDatesTests
{
    [TestFixture]
    public class ToSortableDate
    {
        [TestCase("2016-01-01", "00:00:00", 2016, 1, 1, 0, 0, 0)]
        [TestCase("2016-10-10", "10:10:10", 2016, 10, 10, 10, 10, 10)]
        public void ExtensionsDates_ReturnsEquals(string expectedDate, string expectedTime, int year, int month, int day, int hour,
            int minute, int second)
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
            Assert.Throws<ArgumentNullException>(() =>
            {
                date.ToDateTime();
            });
        }        
        
        [TestCase("2017 10 26")]
        [TestCase("2017-10")]
        [TestCase("2017-10-26 15:35")]
        public void DatesNotParsable_ThrowsException(string date)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                date.ToDateTime();
            });
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

            var rangeActual = Extensions.DateRange(startDate, endDate);

            var rangeExpected  = new List<DateTime>
            {
                new DateTime(2017, 1, 1, 0, 0, 0),
                new DateTime(2017, 1, 2, 0, 0, 0),
                new DateTime(2017, 1, 3, 0, 0, 0),
            };

            CollectionAssert.AreEqual(rangeExpected, rangeActual);
        }
    }
}