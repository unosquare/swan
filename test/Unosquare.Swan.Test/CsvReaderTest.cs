namespace Unosquare.Swan.Test.CsvReaderTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using NUnit.Framework;
    using Formatters;
    using Mocks;

    [TestFixture]
    public abstract class CsvReaderTest
    {
        protected readonly string[] Headers = { "Company", "OpenPositions", "MainTechnology", "Revenue" };

        protected readonly string Data = @"Company,OpenPositions,MainTechnology,Revenue
Co,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 "" 
Ca,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 """;

        protected readonly Dictionary<string, string> Map = new Dictionary<string, string>
        {
            {"Company", "Warsong Clan"},
            {"OpenPositions", "Wolfrider"},
            {"MainTechnology", "Axe"},
            {"Revenue", "$190000G"}
        };
    }

    public class Constructor : CsvReaderTest
    {
        [Test]
        public void WithValidStreamAndValidEncoding_ReturnsReader()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data)))
            {
                var reader = new CsvReader(stream, true, Encoding.ASCII);

                Assert.IsNotNull(reader);
            }
        }

        [Test]
        public void WithNullStream_ThrowsNullReferenceException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var csvReader = new CsvReader(null, true, Encoding.ASCII);
            });
        }

        [Test]
        public void WithNullEncoding_ThrowsNullReferenceException()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data)))
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    var csvReader = new CsvReader(stream, true, null);
                });
            }
        }
    }

    public class SkipRecord : CsvReaderTest
    {
        [Test]
        public void WithValidStream_SkipsRecord()
        {
            var position = 0;

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data)))
            {
                var reader = new CsvReader(stream, Encoding.ASCII);
                reader.SkipRecord();
                Assert.AreNotEqual(stream.Position, position);
            }
        }

        [Test]
        public void WithValidStringAndEscapeCharacter_SkipsRecord()
        {
            var position = 0;
            var data = "Orgrimmar,m";

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(data)))
            {
                var reader = new CsvReader(stream, Encoding.ASCII) { EscapeCharacter = 'm' };

                reader.SkipRecord();

                Assert.AreNotEqual(stream.Position, position);
            }
        }

        [Test]
        public void WithInvalidString_ThrowsEndOfStreamException()
        {
            var tempFile = Path.GetTempFileName();

            var reader = new CsvReader(tempFile);
            Assert.Throws<EndOfStreamException>(() => reader.SkipRecord());
        }
    }

    public class ReadHeadings : CsvReaderTest
    {
        [Test]
        public void WithValidStream_ReturnsAnArray()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data)))
            {
                var reader = new CsvReader(stream);
                var headings = reader.ReadHeadings();
                Assert.IsNotNull(headings);
                Assert.AreEqual(Headers, headings);
            }
        }

        [Test]
        public void WithReadHeadingsAlreadyCalled_ThrowsInvalidOperationException()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data)))
            {
                var reader = new CsvReader(stream);
                reader.ReadHeadings();

                Assert.Throws<InvalidOperationException>(() => reader.ReadHeadings());
            }
        }

        [Test]
        public void WithReadHeadingsAsSecondOperation_ThrowsInvalidOperationException()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data)))
            {
                var reader = new CsvReader(stream);
                reader.ReadLine();

                Assert.Throws<InvalidOperationException>(() => reader.ReadHeadings());
            }
        }
    }

    public class ReadLine : CsvReaderTest
    {
        [Test]
        public void WithValidStream_ReturnsAnArray()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data)))
            {
                var reader = new CsvReader(stream);
                var line = reader.ReadLine();

                Assert.IsNotEmpty(line);
            }
        }

        [Test]
        public void WithInvalidString_ThrowsEndOfStreamException()
        {
            var tempFile = Path.GetTempFileName();

            var reader = new CsvReader(tempFile);
            Assert.Throws<EndOfStreamException>(() => reader.ReadLine());
        }

        [Test]
        public void WithInvalidStringAndEncoding_ThrowsEndOfStreamException()
        {
            var tempFile = Path.GetTempFileName();

            var reader = new CsvReader(tempFile, Definitions.Windows1252Encoding);
            Assert.Throws<EndOfStreamException>(() => reader.ReadLine());
        }
    }

    public class ReadObject : CsvReaderTest
    {
        [Test]
        public void WithValidStream_ReturnsADictionary()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data)))
            {
                var reader = new CsvReader(stream);
                reader.ReadHeadings();
                var readObj = reader.ReadObject();

                Assert.IsNotNull(readObj);
            }
        }

        [Test]
        public void WithoutReadHeadingsCall_ThrowsInvalidOperationException()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data)))
            {
                var reader = new CsvReader(stream);

                Assert.Throws<InvalidOperationException>(() => reader.ReadObject());
            }
        }

        [Test]
        public void WithNullAsParam_ThrowsArgumentNullException()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data)))
            {
                var reader = new CsvReader(stream);
                reader.ReadHeadings();

                Assert.Throws<ArgumentNullException>(() => reader.ReadObject(null));
            }
        }

        [Test]
        public void WithSampleDto_ThrowsArgumentNullException()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data)))
            {
                var reader = new CsvReader(stream);

                Assert.Throws<ArgumentNullException>(() => reader.ReadObject<UserDto>());
            }
        }

        [Test]
        public void WithNullDictionaryAsRef_ThrowsArgumentNullException()
        {
            Dictionary<string, string> refDictionary = null;

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data)))
            {
                var reader = new CsvReader(stream);
                reader.ReadHeadings();

                Assert.Throws<ArgumentNullException>(() => reader.ReadObject(Map, ref refDictionary));
            }
        }

        [Test]
        public void WithInvalidTempFile_ThrowsEndOfStreamException()
        {
            var tempFile = Path.GetTempFileName();
            var reader = new CsvReader(tempFile);
            
            Assert.Throws<EndOfStreamException>(() => reader.ReadObject<UserDto>(Map));
        }

        [Test]
        public void WithNoReadHeadingsCall_ThrowsInvalidOperationException()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data)))
            {
                var reader = new CsvReader(stream);
                Assert.Throws<InvalidOperationException>(() => reader.ReadObject<UserDto>(Map));
            }
        }
    }

    public class Count : CsvReaderTest
    {
        [Test]
        public void WithValidStream_ReturnsNumberOfLinesReaded()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data)))
            {
                var reader = new CsvReader(stream);

                reader.ReadHeadings();

                Assert.AreEqual(1, reader.Count);
            }
        }
    }

    public class EscapeCharacter : CsvReaderTest
    {
        [Test]
        public void WithValidStream_GetsAndSetsSeparatorEscapeCharacter()
        {
            var reader = new CsvReader(new MemoryStream()) { EscapeCharacter = '?' };

            Assert.AreEqual('?', reader.EscapeCharacter);
        }
    }

    public class SeparatorCharacter : CsvReaderTest
    {
        [Test]
        public void WithValidStream_GetsAndSetsSeparatorCharacter()
        {
            var reader = new CsvReader(new MemoryStream()) { SeparatorCharacter = '+' };

            Assert.AreEqual('+', reader.SeparatorCharacter);
        }
    }

    public class Dispose : CsvReaderTest
    {
        [Test]
        public void WithDisposeAlreadyCalled_SetsHasDisposeToTrue()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data)))
            {
                var reader = new CsvReader(stream);
                reader.ReadHeadings();

                var readObj = reader.ReadObject();
                reader.Dispose();
                reader.Dispose();

                Assert.IsNotNull(readObj);
            }
        }
    }
}