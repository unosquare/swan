namespace Swan.Test.ExtensionsStringTest
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;
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
        [TestCase("12", 12)]
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
        public void WithValidParam_ReturnsStringifiedObject(string expected, object input)
        {
            Assert.AreEqual(expected, input.Stringify(), $"Testing with {input}");
        }

        [Test]
        public void WithJsonAsParam_ReturnsStringifiedJson()
        {
            var objectInfoLines = BasicJson.GetDefault().Stringify().ToLines();

            Assert.GreaterOrEqual(8, objectInfoLines.Length);
            Assert.AreEqual("$type           : Swan.Test.Mocks.BasicJson", objectInfoLines[0]);
            Assert.AreEqual("    string,", objectInfoLines[2]);
        }

        [Test]
        public void WithEmptyJsonAsParam_ReturnsStringifiedJson()
        {
            var emptyJson = new EmptyJson();
            var objectInfoLines = emptyJson.Stringify().ToLines();

            Assert.AreEqual("$type           : Swan.Test.Mocks.EmptyJson", objectInfoLines[0]);
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

            Assert.AreEqual("[0]: array[2]", objectInfoLines[0]);
            Assert.AreEqual("    [0]: 1234", objectInfoLines[1]);
            Assert.AreEqual("    [1]: 4321", objectInfoLines[2]);
        }

        [Test]
        public void WithDictionaryOfArraysAsParam_ReturnsStringifiedArray()
        {
            var wordDictionary =
                new Dictionary<string, string[][]> { { "Horde Capitals", new[] { DefaultStringList.ToArray(), DefaultStringList.ToArray() } } };

            var objectInfoLines = wordDictionary.Stringify().ToLines();

            Assert.AreEqual("Horde Capitals  : array[2]", objectInfoLines[0]);
            Assert.AreEqual("        [0]: A", objectInfoLines[2]);
            Assert.AreEqual("        [1]: B", objectInfoLines[3]);
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

            Assert.AreEqual("Tyrande         : object", objectInfoLines[0]);
            Assert.AreEqual("Jaina           : object", objectInfoLines[6]);
            Assert.AreEqual("Liadrin         : object", objectInfoLines[12]);
        }
    }

    [TestFixture]
    public class ToStringInvariant : TestFixtureBase
    {
        [TestCase("", null)]
        [TestCase("Test", "Test")]
        [TestCase("Swan.Test.Mocks.Monkey", typeof(Monkey))]
        public void WithObjectAsParam_ReturnsAString(string expected, object input)
        {
            Assert.AreEqual(expected, input.ToStringInvariant(), $"Testing with {input}");
        }

        [TestCase("Test", "Test")]
        [TestCase("Swan.Test.Mocks.Monkey", typeof(Monkey))]
        public void WithGenericAsParam_ReturnsAString<T>(string expected, T input)
        {
            Assert.AreEqual(expected, input.ToStringInvariant(), $"Testing with {input}");
        }
    }

    [TestFixture]
    public class RemoveControlCharsExcept : TestFixtureBase
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
            Assert.AreEqual(expected, input.RemoveControlCharsExcept(excludeChars), $"Testing with {input}");
        }

        [Test]
        public void WithNullString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => NullString.RemoveControlCharsExcept(null));
        }
    }

    [TestFixture]
    public class RemoveControlChars
    {
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
        public void WithValidParams_ReturnsASlicedString(string expected, string input, int startIndex, int endIndex)
        {
            Assert.AreEqual(expected, input.Slice(startIndex, endIndex), $"Testing with {input}");
        }
    }

    [TestFixture]
    public class SliceLength
    {
        [TestCase("", null, 0, 0)]
        [TestCase("Swan", "ThisIsASwanTest", 7, 4)]
        [TestCase("", "ThisIsASwanTest", 10, 0)]
        public void WithValidParam_ReturnsASubstring(string expected, string input, int startIndex, int length)
        {
            Assert.AreEqual(expected, input.SliceLength(startIndex, length), $"Testing with {input}");
        }
    }

    [TestFixture]
    public class Indent
    {
        [TestCase("", null, 0)]
        [TestCase("     Test", "Test", 5)]
        [TestCase("Test", "Test", 0)]
        public void WithValidParams_ReturnsAnIndentedString(string expected, string input, int spaces)
        {
            Assert.AreEqual(expected, input.Indent(spaces), $"Testing with {input}");
        }
    }

    [TestFixture]
    public class TextPositionAt
    {
        [TestCase(0, 0, null, 0)]
        [TestCase(1, 7, "ThisIsASwanTest", 6)]
        [TestCase(2, 0, "ThisIs\nASwanTest", 6)]
        public void WithValidParams_ReturnsATuple(int firstExpected, int secExpected, string input, int charIndex)
        {
            var expected = Tuple.Create(firstExpected, secExpected);

            Assert.AreEqual(expected, input.TextPositionAt(charIndex), $"Testing with {input}");
        }
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
            if (SwanRuntime.OS != Swan.OperatingSystem.Windows)
                Assert.Ignore("Ignored");

            Assert.AreEqual(expected, input.ToSafeFilename(), $"Testing with {input}");
        }

        [Test]
        public void WithNullString_ThrowsArgumentNullException()
        {
            if (SwanRuntime.OS != Swan.OperatingSystem.Windows)
                Assert.Ignore("Ignored");

            Assert.Throws<ArgumentNullException>(() => NullString.ToSafeFilename());
        }
    }

    [TestFixture]
    public class FormatBytes
    {
        [TestCase("2 KB", 2048)]
        [TestCase("97.66 KB", 100000)]
        [TestCase("3.38 MB", 3546346)]
        [TestCase("4.94 TB", 5432675475323)]
        public void WithUlongAsParam_ReturnsFormattedBytes(string expected, long input)
        {
            Assert.AreEqual(expected, ((ulong)input).FormatBytes(), $"Testing with {input}");
        }

        [TestCase("3 KB", 3072)]
        [TestCase("52.2 KB", 53453)]
        [TestCase("639.32 KB", 654664)]
        [TestCase("80.72 MB", 84645653)]
        public void WithLongParam_ReturnsFormattedBytes(string expected, long input)
        {
            Assert.AreEqual(expected, input.FormatBytes(), $"Testing with {input}");
        }
    }

    [TestFixture]
    public class Truncate
    {
        [TestCase("ThisIs", "ThisIsASwanTest", 6)]
        [TestCase("ThisIsASwanTest", "ThisIsASwanTest", 60)]
        [TestCase(null, null, 60)]
        public void WithValidString_ReturnsTruncatedString(string expected, string input, int maximumLength)
        {
            Assert.AreEqual(expected, input.Truncate(maximumLength), $"Testing with {input}");
        }
    }

    [TestFixture]
    public class Contains
    {
        [TestCase(new[] { 'l' })]
        [TestCase(new[] { 'l', 'W' })]
        public void WithValid_ReturnsTrue(params char[] chars)
        {
            Assert.IsTrue("Hello World".Contains(chars));
        }
    }

    [TestFixture]
    public class Hex2Int
    {
        [TestCase(10, 'A')]
        [TestCase(15, 'F')]
        [TestCase(3, '3')]
        public void WithValidChar_ReturnsAsInt(int expected, char input)
        {
            Assert.AreEqual(expected, input.Hex2Int(), $"Testing with {input}");
        }
    }
}