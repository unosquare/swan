namespace Swan.Test;

using NUnit.Framework;
using Swan.Gizmos;

[TestFixture]
public class RangeExtensionsTest
{
    [Test]
    public void WithRangeLookUp_GetsClosestToRequestedValue()
    {
        var range = new RangeLookup<int, string>
        {
            { 0 , "zero" },
            { 1 , "one" },
            { 4 , "four" },
            { 5 , "five" }
        };
        range.TryGetValue(2, out string outValue);

        Assert.AreEqual(range.FirstOrDefault(keys => keys.Key == 1).Value, outValue);
    }
}
