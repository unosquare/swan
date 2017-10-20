using NUnit.Framework;
using System;
using System.IO;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test.ExtensionsStringTest
{
    [TestFixture]
    public class ExtensionsStringTest
    {
        public class Humanize
        {
            [TestCase("Camel Case", "CamelCase")]
            [TestCase("Snake Case", "Snake_Case")]
            public void WithValidString_ReturnsHumanizedString(string expected, string input)
            {
                Assert.AreEqual(expected, input.Humanize(), $"Testing with {input}");
            }
        }

        public class ComputeMD5
        {
            [TestCase(5000)]
            [TestCase(1234)]
            public void WithValidStream_ReturnsMD5(int stream)
            {
                var input = new MemoryStream(new byte[stream]);

                Assert.IsNotNull(input.ComputeMD5().ToDashedHex());
                
            }
            
            [Test]
            public void WithValidString_ReturnsMD5()
            {
                const string input = "HOLA";

                Assert.AreEqual("C6-F0-09-88-43-0D-BC-8E-83-A7-BC-7A-B5-25-63-46", input.ComputeMD5().ToDashedHex(), "Get MD5");
            }
        }

        public class ComputeSha1
        {
            [Test]
            public void WithValidString_ReturnsSha1()
            {
                const string input = "HOLA";

                Assert.AreEqual("261C5AD45770CC14875C8F46EAA3ECA42568104A", input.ComputeSha1().ToUpperHex(), "Get Sha1");
            }
        }

        public class ComputeSha256
        {
            [Test]
            public void WithValidString_ReturnsSha256()
            {
                const string input = "HOLA";

                Assert.AreEqual("73C3DE4175449987EF6047F6E0BEA91C1036A8599B43113B3F990104AB294A47".ConvertHexadecimalToBytes(), input.ComputeSha256(), "Get Sha256");
            }
        }

        public class ComputeSha512
        {
            [Test]
            public void WithValidString_ReturnsSha512()
            {
                const string input = "HOLA";

                Assert.AreEqual("XPWJJ7QTeLzAdrJrO4UKZuvOw6znT2uUnaVAVyHdOUiKI49a//eTtRJQOLsd1xhMHBHEf0hE0cy7MQycdYk7ZQ==", input.ComputeSha512().ToBase64(), "Get Sha512");
            }
        }

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

                Assert.Greater(7, objectInfoLines.Length);
                Assert.AreEqual("$type           : Unosquare.Swan.Test.Mocks.BasicJson", objectInfoLines[0]);
                Assert.AreEqual("StringData      : string", objectInfoLines[1]);
                Assert.AreEqual("IntData         : 1", objectInfoLines[2]);
            }
        }

        public class ToStringInvariant
        {
            [TestCase("", null)]
            [TestCase("Test", "Test")]
            [TestCase("Unosquare.Swan.Test.Mocks.Monkey", typeof(Monkey))]
            public void WithObjectAsParam_ReturnsAString(string expected, object input)
            {
                Assert.AreEqual(expected, input.ToStringInvariant(), $"Testing with {input}");
            }
        }

        public class RemoveControlCharsExcept
        {
            [TestCase("Test", "Test", null)]
            [TestCase("Test", "\0Test\0", null)]
            [TestCase("\0Test", "\0Test", new char[] { '\0' })]
            [TestCase("\0Test", "\0Test\t", new char[] { '\0' })]
            public void WithValidString_ReturnsStringWithoutControlCharacters(string expected, string input, char[] excludeChars)
            {
                Assert.AreEqual(expected, input.RemoveControlCharsExcept(excludeChars), $"Testing with {input}");
            }
        }

        public class RemoveControlChars
        {
            [Test]
            public void WithValidString_ReturnsStringWithoutControlCharacters()
            {
                const string input = "\0Test\t";
                Assert.AreEqual("Test", input.RemoveControlChars(), $"Testing with {input}");
            }
        }

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

        public class ToSafeFilename
        {
            [TestCase("FileName", ":File|Name*")]
            [TestCase("LongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongF", "LongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileNameLongFileName")]
            public void WithValidParams_ReturnsASafeFileName(string expected, string input)
            {
                if(Runtime.OS != OperatingSystem.Windows)
                {
                    Assert.Ignore("Ignored");
                }
                else
                {
                    Assert.AreEqual(expected, input.ToSafeFilename(), $"Testing with {input}");
                }
            }
        }

        public class FormatBytes
        {
            [Test]
            public void WithValidParam_ReturnsFormatedBytes()
            {
                const ulong input = 2048;
                Assert.AreEqual("2 KB", input.FormatBytes(), $"Testing with {input}");
            }
        }

        public class Truncate
        {
            [TestCase("ThisIs", "ThisIsASwanTest", 6)]
            [TestCase("ThisIsASwanTest", "ThisIsASwanTest", 60)]
            public void WithValidString_ReturnsTruncatedString(string expected, string input, int maximumLength)
            {
                Assert.AreEqual(expected, input.Truncate(maximumLength), $"Testing with {input}");
            }
        }

        public class Contains
        {
            [Test]
            public void WithValidString_ReturnsTrue()
            {
                var input = "Hello World";
                Assert.IsTrue(input.Contains('l'));
            }
        }

    }
}
