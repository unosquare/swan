using System;
using NUnit.Framework;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ExtensionsDatesTest
    {
        [TestCase("2016-01-01", "00:00:00", 2016, 1, 1, 0, 0, 0)]
        [TestCase("2016-10-10", "10:10:10", 2016, 10, 10, 10, 10, 10)]
        public void SortableDatesTest(string expectedDate, string expectedTime, int year, int month, int day, int hour,
            int minute, int second)
        {
            var input = new DateTime(year, month, day, hour, minute, second);
            Assert.AreEqual(expectedDate, input.ToSortableDate());
            Assert.AreEqual($"{expectedDate} {expectedTime}", input.ToSortableDateTime());

            Assert.AreEqual(input, $"{expectedDate} {expectedTime}".ToDateTime());
        }
    }
}