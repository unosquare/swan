namespace Swan.Test;

using Extensions;
using Formatters;
using Mocks;

[TestFixture]
public class Humanize
{
    [TestCase("Camel Case", "CamelCase")]
    [TestCase("Snake Case", "Snake_Case")]
    public void WithValidString_ReturnsHumanizedString(string expected, string input) =>
        Assert.AreEqual(expected, input.Humanize(), $"Testing with {input}");

    [TestCase("Yes", true)]
    [TestCase("No", false)]
    public void WithValidBoolean_ReturnsHumanizedString(string expected, bool input) =>
        Assert.AreEqual(expected, input.Humanize(), $"Testing with {input}");

    [TestCase("Camel Case", "CamelCase")]
    [TestCase("Yes", true)]
    [TestCase("(null)", null)]
    [TestCase("(Stringified)   : 12", 12)]
    public void WithValidObject_ReturnsHumanizedString(string expected, object input) =>
        Assert.AreEqual(expected, input.Humanize(), $"Testing with {input}");
}

[TestFixture]
public class ReplaceAll
{
    [TestCase("Cam lCas ", "CamelCase", new[] { 'e' }, " ")]
    [TestCase("CamelCase", "CxamxelCxaxse", new[] { 'x' }, "")]
    public void WithValidString_ReturnsStringWithReplacedCharacters(string expected, string input, char[] toBeReplaced, string toReplace) =>
        Assert.AreEqual(expected, input.ReplaceAll(toReplace, toBeReplaced), $"Testing with {input}");
}

[TestFixture]
public class Stringify : TestFixtureBase
{
    [TestCase("string", "string")]
    [TestCase("(null)", null)]
    public void WithValidParam_ReturnsStringifiedObject(string expected, object input) => Assert.IsTrue(input.Stringify().EndsWith(expected), $"Testing with {input}");

    [Test]
    public void WithJsonAsParam_ReturnsStringifiedJson()
    {
        var family = SampleFamily.Create(true);
        var familyString = family.Stringify();

        var s = "hello world".Stringify();

        var objectInfoLines = BasicJson.GetDefault().Stringify().ToLines();

        Assert.GreaterOrEqual(7, objectInfoLines.Length);
        Assert.AreEqual("  DecimalData     : 10.33", objectInfoLines[4]);
    }

    [Test]
    public void WithEmptyJsonAsParam_ReturnsStringifiedJson()
    {
        var emptyJson = new EmptyJson();
        var objectInfoLines = emptyJson.Stringify().ToLines();

        Assert.IsTrue(objectInfoLines[0].Length > 0);
    }

    [Test]
    public void WithListOfArraysAsParam_ReturnsStringifiedArray()
    {
        var arrayInt = new[] { 1234, 4321 };

        var arrayList = new List<int[]>
        {
            arrayInt,
            arrayInt,
        };

        var objectInfoLines = arrayList.Stringify().ToLines();

        StringAssert.Contains("(Stringified)", objectInfoLines[0]);
        Assert.AreEqual("  [0]: ", objectInfoLines[1]);
        Assert.AreEqual("    [0]: 1234", objectInfoLines[2]);
        Assert.AreEqual("    [1]: 4321", objectInfoLines[3]);
    }

    [Test]
    public void WithDictionaryOfArraysAsParam_ReturnsStringifiedArray()
    {
        var wordDictionary =
            new Dictionary<string, string[][]> { { "Horde Capitals", new[] { DefaultStringList.ToArray(), DefaultStringList.ToArray() } } };

        var objectInfoLines = wordDictionary.Stringify().ToLines();

        Assert.AreEqual("  Horde Capitals  : ", objectInfoLines[1]);
        Assert.AreEqual("    [1]: ", objectInfoLines[6]);
        Assert.AreEqual("      [0]: A", objectInfoLines[7]);
    }

    [Test]
    public void WithDictionaryOfDictionariesAsParam_ReturnsStringifiedArray()
    {
        var persons = new Dictionary<string, Dictionary<int, string>>
        {
            {"Tyrande", DefaultDictionary },
            {"Jaina", DefaultDictionary },
            {"Liadrin", DefaultDictionary },
        };

        var objectInfoLines = persons.Stringify().ToLines();

        Assert.IsTrue(objectInfoLines[1].StartsWith("  Tyrande         : "));
        Assert.IsTrue(objectInfoLines[7].StartsWith("  Jaina           : "));
        Assert.IsTrue(objectInfoLines[13].StartsWith("  Liadrin         : "));
    }
}

[TestFixture]
public class ToStringInvariant : TestFixtureBase
{
    [TestCase("", null)]
    [TestCase("Test", "Test")]
    [TestCase("Swan.Test.Mocks.Monkey", typeof(Monkey))]
    public void WithObjectAsParam_ReturnsAString(string expected, object input) => Assert.AreEqual(expected, input.ToStringInvariant(), $"Testing with {input}");

    [TestCase("Test", "Test")]
    [TestCase("Swan.Test.Mocks.Monkey", typeof(Monkey))]
    public void WithGenericAsParam_ReturnsAString<T>(string expected, T input) => Assert.AreEqual(expected, input.ToStringInvariant(), $"Testing with {input}");
}

[TestFixture]
public class RemoveControlChars : TestFixtureBase
{
    [TestCase("Test", "Test", null)]
    [TestCase("Test", "\0Test\0", null)]
    [TestCase("\0Test", "\0Test", new[] { '\0' })]
    [TestCase("\0Test", "\0Test\t", new[] { '\0' })]
    public void WithValidString_ReturnsStringWithoutControlCharacters(
        string expected,
        string input,
        char[] excludeChars)
    {
        var output = input.RemoveControlChars(excludeChars);
        Assert.AreEqual(expected, output, $"Testing with {input}");
    }

    [Test]
    public void WithValidString_ReturnsStringWithoutControlCharacters()
    {
        const string input = "\0Test\t";
        Assert.AreEqual("Test", input.RemoveControlChars(), $"Testing with {input}");
    }
}

[TestFixture]
public class Slice
{
    [TestCase("", null, 0, 0)]
    [TestCase("Swan", "ThisIsASwanTest", 7, 10)]
    [TestCase("", "ThisIsASwanTest", 10, 7)]
    public void WithValidParams_ReturnsASlicedString(string expected, string input, int startIndex, int endIndex) => Assert.AreEqual(expected, input.Slice(startIndex, endIndex), $"Testing with {input}");
}

[TestFixture]
public class SliceLength
{
    [TestCase("", null, 0, 0)]
    [TestCase("Swan", "ThisIsASwanTest", 7, 4)]
    [TestCase("", "ThisIsASwanTest", 10, 0)]
    public void WithValidParam_ReturnsASubstring(string expected, string input, int startIndex, int length) => Assert.AreEqual(expected, input.SliceLength(startIndex, length), $"Testing with {input}");
}

[TestFixture]
public class ToSafeFilename : TestFixtureBase
{
    [TestCase("FileName", ":File|Name*")]
    [TestCase(
        "LongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongF",
        "LongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileName")]
    public void WithValidParams_ReturnsASafeFileName(string expected, string input)
    {
        if (!OperatingSystem.IsWindows())
            Assert.Ignore("Ignored");

        Assert.AreEqual(expected, input.ToSafeFilename(), $"Testing with {input}");
    }

    [Test]
    public void WithNullString_ThrowsArgumentNullException()
    {
        if (!OperatingSystem.IsWindows())
            Assert.Ignore("Ignored");

        Assert.Throws<ArgumentNullException>(() => NullString.ToSafeFilename());
    }
}

[TestFixture]
public class Truncate
{
    [TestCase("ThisIs", "ThisIsASwanTest", 6)]
    [TestCase("ThisIsASwanTest", "ThisIsASwanTest", 60)]
    [TestCase(null, null, 60)]
    public void WithValidString_ReturnsTruncatedString(string expected, string input, int maximumLength) =>
        Assert.AreEqual(expected, input.Truncate(maximumLength), $"Testing with {input}");
}

[TestFixture]
public class Contains
{
    [TestCase(new[] { 'l' })]
    [TestCase(new[] { 'l', 'W' })]
    public void WithValid_ReturnsTrue(params char[]? chars) => Assert.IsTrue("Hello World".Contains(chars));
}
