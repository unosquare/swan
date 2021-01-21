namespace Swan.Test.ExtensionsDatesTests
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
                new(2017, 1, 1, 0, 0, 0),
                new(2017, 1, 2, 0, 0, 0),
                new(2017, 1, 3, 0, 0, 0),
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
            var date = new DateTime(2017, 10, 27).ToUniversalTime().Date;

            Assert.AreEqual(1509062400, date.ToUnixEpochDate());
        }
    }

    [TestFixture]
    public class CompareDates
    {
        private readonly DateTime _date = new(2002, 7, 3, 12, 0, 0, 200);

        [Test]
        public void WithFullDateTimes_ReturnsDateTimeSpan()
        {
            var result = _date.GetDateTimeSpan(new DateTime(1969, 8, 15, 5, 7, 10, 100));

            Assert.That(result.Years, Is.EqualTo(32));
            Assert.That(result.Months, Is.EqualTo(10));
            Assert.That(result.Days, Is.EqualTo(18));
            Assert.That(result.Hours, Is.EqualTo(6));
            Assert.That(result.Minutes, Is.EqualTo(52));
            Assert.That(result.Seconds, Is.EqualTo(50));
            Assert.That(result.Milliseconds, Is.EqualTo(100));
        }

        [Test]
        public void WithPartialDateTimes_ReturnsDateTimeSpan()
        {
            var result = new DateTime(1969, 8, 15).GetDateTimeSpan(_date);

            Assert.AreEqual(result.Years, 32);
            Assert.AreEqual(result.Months, 10);
            Assert.AreEqual(result.Days, 18);
            Assert.AreEqual(result.Minutes, 0);
            Assert.AreEqual(result.Seconds, 0);
            Assert.AreEqual(result.Milliseconds, 200);
        }
    }

    [TestFixture]
    public class AsCronCanRun
    {
        private readonly DateTime _date = new(2018, 7, 3, 11, 25, 0);

        [Test]
        public void WithNonNull_Minute()
        {
            Assert.IsTrue(_date.AsCronCanRun(25));
        }

        [Test]
        public void WithNonNull_Minute_EveryMinute()
        {
            Assert.IsTrue(_date.AsCronCanRun(minute: "*/5"));
        }

        [Test]
        public void WithNonNull_Hour()
        {
            Assert.IsTrue(_date.AsCronCanRun(hour: 11));
        }

        [Test]
        public void WithNonNull_Hour_InHourRange()
        {
            Assert.IsTrue(_date.AsCronCanRun(hour: "10-12"));
        }

        [Test]
        public void WithNonNull_DayOfMonth()
        {
            Assert.IsTrue(_date.AsCronCanRun(dayOfMonth: 3));
        }

        [Test]
        public void WithNonNull_DayOfMonth_InDayMonthSeries()
        {
            Assert.IsTrue(_date.AsCronCanRun(dayOfMonth: "3,4,5"));
        }

        [Test]
        public void WithNonNull_Month()
        {
            Assert.IsTrue(_date.AsCronCanRun(month: 7));
        }

        [Test]
        public void WithNonNull_DayOfWeek()
        {
            Assert.IsTrue(_date.AsCronCanRun(dayOfWeek: 2));
        }

        [Test]
        public void WithNonNull_Minute_Hour()
        {
            Assert.IsTrue(_date.AsCronCanRun(25, 11));
        }

        [Test]
        public void WithNonNull_Minute_Hour_WithHourRange()
        {
            Assert.IsTrue(_date.AsCronCanRun(minute: "25", hour: "10-15"));
        }

        [Test]
        public void WithNonNull_Minute_Hour_DayOfMonth()
        {
            Assert.IsTrue(_date.AsCronCanRun(25, 11, 3));
        }

        [Test]
        public void WithNonNull_Minute_Hour_DayOfMonth_WithDayOfMonthSeries()
        {
            Assert.IsTrue(_date.AsCronCanRun(minute: "25", hour: "11", dayOfMonth: "2,3,4"));
        }

        [Test]
        public void WithNonNull_Minute_Hour_DayOfMonth_Month()
        {
            Assert.IsTrue(_date.AsCronCanRun(25, 11, 3, 7));
        }

        [Test]
        public void WithNonNull_Minute_Hour_DayOfMonth_Month_DayOfWeek()
        {
            Assert.IsTrue(_date.AsCronCanRun(25, 11, 3, 7, 2));
        }

        [Test]
        public void WithWrongParams_Throws_FormatException()
        {
            Assert.Throws<FormatException>(() =>
            {
                _date.AsCronCanRun(minute: "hello");
            });
        }
    }

    [TestFixture]
    public class ToRfc1123String
    {
        private readonly DateTime _date = new(2002, 7, 3, 12, 0, 0, 200, DateTimeKind.Utc);

        [Test]
        public void WithValidDate_ReturnsRfc1123String()
        {
            Assert.AreEqual("Wed, 03 Jul 2002 12:00:00 GMT", _date.ToRfc1123String());
        }
    }
}
