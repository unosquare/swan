using System;
using NUnit.Framework;

namespace Unosquare.Swan.Test.ExtensionsDatesTests
{
    public abstract class ExtensionsDatesTest
    {

    }

    [TestFixture]
    public class ToSortableDate : ExtensionsDatesTest
    {
        [Test]
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
    public class ToDateTime : ExtensionsDatesTest
    {
        [Test]
        public void WithNullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var date = Extensions.ToDateTime(null);
            });
        }

        [Test]
        public void WithEmptyValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var date = Extensions.ToDateTime("");
            });
        }

        [Test]
        public void WithWhiteSpaceValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var date = Extensions.ToDateTime(" ");
            });
        }

        [Test]
        public void DateTimePartsWithDifferentLength_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var date = Extensions.ToDateTime("2017 10 26");
            });
        }

        [Test]
        public void DatePartsDifferentLengthToThree_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var date = Extensions.ToDateTime("2017-10");
            });
        }
    }
}