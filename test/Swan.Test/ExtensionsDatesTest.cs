namespace Swan.Test;

using Extensions;

[TestFixture]
public class ToDateTime
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void InvalidArguments_ThrowsArgumentNullException(string date) => Assert.Throws<ArgumentNullException>(() => date.ToDateTime());

    [TestCase("2017 10 26")]
    [TestCase("2017-10")]
    [TestCase("2017-10-26 15:35")]
    public void DatesNotParseable_ThrowsException(string date) => Assert.Throws<ArgumentException>(() => date.ToDateTime());
}
