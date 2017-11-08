using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test.ExtensionsStringTest
{
    [TestFixture]
    public class Humanize
    {
        [TestCase("Camel Case", "CamelCase")]
        [TestCase("Snake Case", "Snake_Case")]
        public void WithValidString_ReturnsHumanizedString(string expected, string input)
        {
            Assert.AreEqual(expected, input.Humanize(), $"Testing with {input}");
        }
    }

    [TestFixture]
    public class ComputeMD5
    {
        [TestCase("6B-F9-5A-48-F3-66-BD-F8-AF-3A-19-8C-7B-72-3C-77", 5000)]
        [TestCase("9B-4C-8A-5E-36-D3-BE-7E-2C-4B-1D-75-DE-D8-C8-A1", 1234)]
        [TestCase("45-9B-B4-0F-7B-36-1B-90-41-4A-72-D4-0B-5A-0E-D5", 53454)]
        public void WithValidStream_ReturnsMD5(string expected, int stream)
        {
            var input = new MemoryStream(new byte[stream]);

            Assert.AreEqual(expected, input.ComputeMD5().ToDashedHex());
        }

        [TestCase("50-82-83-2F-01-E0-EB-94-30-C9-DB-6E-AE-FE-BC-72", "Illidan")]
        [TestCase("8A-B3-AF-D6-A4-82-35-06-10-78-20-FA-EB-85-5E-B4", "Arthas")]
        [TestCase("8C-1D-5F-69-43-84-06-3D-F2-E0-66-4F-54-9C-4B-93", "Grommash")]
        public void WithValidString_ReturnsMD5(string expected, string input)
        {
            Assert.AreEqual(expected, input.ComputeMD5().ToDashedHex(), "Get MD5");
        }

        [Test]
        public void WithNull_ReturnsMD5()
        {
            MemoryStream input = null;

            Assert.Throws<ArgumentNullException>(() => input.ComputeMD5());
        }
    }

    [TestFixture]
    public class ComputeSha1
    {
        [TestCase("06636F8D82BDEB41C444F82D2EBCF431FC31FE12", "Suramar")]
        [TestCase("0E3EB0AF296788BC24DD29BC3C767EE6A829D473", "Stormwind")]
        [TestCase("D4570F48B4B7B720B55499B5D01A0215A6A60FB2", "Darnassus")]
        public void WithValidString_ReturnsSha1(string expected, string input)
        {
            Assert.AreEqual(expected, input.ComputeSha1().ToUpperHex(), "Get Sha1");
        }
    }

    [TestFixture]
    public class ComputeSha256
    {
        [Test]
        public void WithValidString_ReturnsSha256()
        {
            const string input = "HOLA";

            Assert.AreEqual(
                "73C3DE4175449987EF6047F6E0BEA91C1036A8599B43113B3F990104AB294A47".ConvertHexadecimalToBytes(),
                input.ComputeSha256());
        }
    }

    [TestFixture]
    public class ComputeSha512
    {
        [TestCase("uG16jy5/N+hPwel+4xRVtOfyCZ56K9Ds0SF4GE9oQgYRBGzTeAD+h94cIgc6ROyNjbK6wBFVhgqqjDDh01f4rg==",
            "Eastern Kingdoms")]
        [TestCase("rDh3voP2/h+S/mDAjnsf8MFRM+Hst6mTxB+rxehSA2KW5fUR2hSBNO9AGGifOzUuWPrO0OOpE0nskGPUw2q+iQ==",
            "Northrend")]
        [TestCase("f1WQUrni0kEMcQ0u8tYkvC17zWphJYzQqdWHsrXRuUBoSG+MUrLx3urczB+zkJ9OFbCqUyjCV6NqMEViv7drqg==",
            "Pandaria")]
        public void WithValidString_ReturnsSha512(string expected, string input)
        {
            Assert.AreEqual(expected, input.ComputeSha512().ToBase64(), "Get Sha512");
        }
    }

    [TestFixture]
    public class Stringify
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
            Assert.AreEqual("$type           : Unosquare.Swan.Test.Mocks.BasicJson", objectInfoLines[0]);
            Assert.AreEqual("IntData         : 1", objectInfoLines[2]);
        }

        [Test]
        public void WithEmptyJsonAsParam_ReturnsStringifiedJson()
        {
            var emptyJson = new EmptyJson();
            var objectInfoLines = emptyJson.Stringify().ToLines();

            Assert.AreEqual("$type           : Unosquare.Swan.Test.Mocks.EmptyJson", objectInfoLines[0]);
        }

        [Test]
        public void WithListOfArraysAsParam_ReturnsStringifiedArray()
        {
            var arrayInt = new[] {1234, 4321};

            var arrayList = new List<int[]>
            {
                arrayInt,
                arrayInt
            };

            var objectInfoLines = arrayList.Stringify().ToLines();

            Assert.AreEqual("[0]: array[2]", objectInfoLines[0]);
            Assert.AreEqual("    [0]: 1234", objectInfoLines[1]);
            Assert.AreEqual("    [1]: 4321", objectInfoLines[2]);
        }

        [Test]
        public void WithDictionaryOfArraysAsParam_ReturnsStringifiedArray()
        {
            string[] arrayString = {"Orgrimmar", "Thuder Bluff", "Undercity", "Silvermoon", null};

            var wordDictionary =
                new Dictionary<string, string[][]> {{"Horde Capitals", new[] {arrayString, arrayString}}};

            var objectInfoLines = wordDictionary.Stringify().ToLines();

            Assert.AreEqual("Horde Capitals  : array[2]", objectInfoLines[0]);
            Assert.AreEqual("        [0]: Orgrimmar", objectInfoLines[2]);
            Assert.AreEqual("        [1]: Thuder Bluff", objectInfoLines[3]);
        }

        [Test]
        public void WithDictionaryOfDictionariesAsParam_ReturnsStringifiedArray()
        {
            var persons = new Dictionary<string, Dictionary<string, string>>
            {
                {"Tyrande", new Dictionary<string, string> {{"Race", "Night Elf\r"}, {"Affiliation", "Alliance\r"}}},
                {"Jaina", new Dictionary<string, string> {{"Race", "Human\r"}, {"Affiliation", "Alliance\r"}}},
                {"Liadrin", new Dictionary<string, string> {{"Race", "Blood Elf\n"}, {"Affiliation", "Horde\n"}}}
            };

            var objectInfoLines = persons.Stringify().ToLines();

            Assert.AreEqual("Tyrande         : object", objectInfoLines[0]);
            Assert.AreEqual("Jaina           : object", objectInfoLines[5]);
            Assert.AreEqual("Liadrin         : object", objectInfoLines[10]);
        }

    }

    [TestFixture]
    public class ToStringInvariant
    {
        [TestCase("", null)]
        [TestCase("Test", "Test")]
        [TestCase("Unosquare.Swan.Test.Mocks.Monkey", typeof(Monkey))]
        public void WithObjectAsParam_ReturnsAString(string expected, object input)
        {
            Assert.AreEqual(expected, input.ToStringInvariant(), $"Testing with {input}");
        }

        [TestCase("Test", "Test")]
        [TestCase("Unosquare.Swan.Test.Mocks.Monkey", typeof(Monkey))]
        public void WithGenericAsParam_ReturnsAString<T>(string expected, T input)
        {
            Assert.AreEqual(expected, input.ToStringInvariant(), $"Testing with {input}");
        }
    }

    [TestFixture]
    public class RemoveControlCharsExcept
    {
        [TestCase("Test", "Test", null)]
        [TestCase("Test", "\0Test\0", null)]
        [TestCase("\0Test", "\0Test", new[] {'\0'})]
        [TestCase("\0Test", "\0Test\t", new[] {'\0'})]
        public void WithValidString_ReturnsStringWithoutControlCharacters(string expected, string input,
            char[] excludeChars)
        {
            Assert.AreEqual(expected, input.RemoveControlCharsExcept(excludeChars), $"Testing with {input}");
        }

        [Test]
        public void WithNullString_ThrowsArgumentNullException()
        {
            string input = null;

            Assert.Throws<ArgumentNullException>(() => input.RemoveControlCharsExcept(null));
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
    public class ToSafeFilename
    {
        [TestCase("FileName", ":File|Name*")]
        [TestCase(
            "LongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongF",
            "LongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileName")]
        public void WithValidParams_ReturnsASafeFileName(string expected, string input)
        {
            if (Runtime.OS != OperatingSystem.Windows)
                Assert.Ignore("Ignored");

            Assert.AreEqual(expected, input.ToSafeFilename(), $"Testing with {input}");
        }

        [Test]
        public void WithNullString_ThrowsArgumentNullException()
        {
            if (Runtime.OS != OperatingSystem.Windows)
                Assert.Ignore("Ignored");

            string input = null;
            Assert.Throws<ArgumentNullException>(() => input.ToSafeFilename());
        }
    }

    [TestFixture]
    public class FormatBytes
    {
        [TestCase("2 KB", 2048)]
        [TestCase("97.66 KB", 100000)]
        [TestCase("3.38 MB", 3546346)]
        [TestCase("4.94 TB", 5432675475323)]
        public void WithUlongAsParam_ReturnsFormatedBytes(string expected, long input)
        {
            var inputByte = Convert.ToUInt64(input);

            Assert.AreEqual(expected, inputByte.FormatBytes(), $"Testing with {input}");
        }

        [TestCase("3 KB", 3072)]
        [TestCase("52.2 KB", 53453)]
        [TestCase("639.32 KB", 654664)]
        [TestCase("80.72 MB", 84645653)]
        public void WithLongParam_ReturnsFormatedBytes(string expected, long input)
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
        [TestCase(new[] {'l'})]
        [TestCase(new[] {'l', 'W'})]
        public void WithValidString_ReturnsTrue(params char[] chars)
        {
            const string input = "Hello World";
            Assert.IsTrue(input.Contains(chars));
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