using NUnit.Framework;
using Swan.Formatters;
using Swan.Platform;
using Swan.Test.Mocks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Swan.Test.CsvReaderTest
{
    [TestFixture]
    public abstract class CsvReaderTest
    {
        protected readonly string[] Headers = { "Company", "OpenPositions", "MainTechnology", "Revenue" };

        protected readonly string Data = @"Company,OpenPositions,MainTechnology,Revenue
Co,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 "" 
Ca,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 """;

        protected readonly Dictionary<string, string> Map = new()
        {
            { "Company", "Warsong Clan" },
            { "OpenPositions", "Wolfrider" },
            { "MainTechnology", "Axe" },
            { "Revenue", "$190000G" },
        };
    }

    public class Constructor : CsvReaderTest
    {
        [Test]
        public void WithValidStreamAndValidEncoding_ReturnsReader()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
            var reader = new CsvReader(stream, Encoding.ASCII);

            Assert.IsNotNull(reader);
        }

        [Test]
        public void WithNullStream_ThrowsNullReferenceException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var csvReader = new CsvReader(default(MemoryStream), Encoding.ASCII);
            });
        }

        [Test]
        public void WithNullEncoding_ThrowsNullReferenceException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var csvReader = new CsvReader(default(MemoryStream));
            });
        }
    }

    public class SkipRecord : CsvReaderTest
    {
        [Test]
        public void WithValidStream_SkipsRecord()
        {
            var position = 0;

            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
            var reader = new CsvReader(stream, Encoding.ASCII);
            reader.Skip();
            Assert.AreNotEqual(stream.Position, position);
        }

        [Test]
        public void WithValidStringAndEscapeCharacter_SkipsRecord()
        {
            var position = 0;
            var data = "Orgrimmar,m";

            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
            var reader = new CsvReader(stream, Encoding.ASCII, escapeChar: 'm');

            reader.Skip();

            Assert.AreNotEqual(stream.Position, position);
        }

        [Test]
        public void WithInvalidString_ThrowsEndOfStreamException()
        {
            var tempFile = Path.GetTempFileName();

            var reader = new CsvReader(tempFile);
            Assert.Throws<EndOfStreamException>(() => reader.Skip());
        }
    }

    public class ReadHeadings : CsvReaderTest
    {
        [Test]
        public void WithValidStream_ReturnsAnArray()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
            var reader = new CsvDictionaryReader(stream);
            var headings = reader.ReadHeadings().Current.ToArray();
            Assert.IsNotEmpty(headings);
            Assert.AreEqual(Headers, headings);
        }

        [Test]
        public void WithReadHeadingsAlreadyCalled_ThrowsInvalidOperationException()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
            using var reader = new CsvDictionaryReader(stream);
            reader.ReadHeadings();

            Assert.Throws<InvalidOperationException>(() => reader.ReadHeadings());
        }

        [Test]
        public void WithReadHeadingsAsSecondOperation_ThrowsInvalidOperationException()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
            using var reader = new CsvDictionaryReader(stream);
            _ = reader.ReadHeadings();

            Assert.Throws<InvalidOperationException>(() => reader.ReadHeadings());
        }
    }

    public class ReadLine : CsvReaderTest
    {
        [Test]
        public void WithValidStream_ReturnsAnArray()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
            var reader = new CsvReader(stream);
            var line = reader.Read();

            Assert.IsNotEmpty(line);
        }

        [Test]
        public void WithInvalidString_ThrowsEndOfStreamException()
        {
            var tempFile = Path.GetTempFileName();

            var reader = new CsvReader(tempFile);
            Assert.Throws<EndOfStreamException>(() => reader.Read());
        }

        [Test]
        public void WithInvalidStringAndEncoding_ThrowsEndOfStreamException()
        {
            var tempFile = Path.GetTempFileName();

            var reader = new CsvReader(tempFile, SwanRuntime.Windows1252Encoding);
            Assert.Throws<EndOfStreamException>(() => reader.Read());
        }
    }

    public class ReadObject : CsvReaderTest
    {
        [Test]
        public void WithValidStream_ReturnsADictionary()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
            using var reader = new CsvDictionaryReader(stream);
            var readObj = reader.ReadObject() as IDictionary;
            Assert.IsNotNull(readObj);
        }

        [Test]
        public void WithoutReadHeadingsCall_ThrowsInvalidOperationException()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
            using var reader = new CsvDictionaryReader(stream);

            Assert.Throws<InvalidOperationException>(() =>
            {
                reader.SetHeadings("").SetHeadings().ReadObject();
            });
        }

        [Test]
        public void WithNullAsParam_ThrowsArgumentNullException()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
            using var reader = new CsvDictionaryReader(stream);
            Assert.Throws<ArgumentNullException>(() => reader.ReadInto(null));
        }

        [Test]
        public void WithSampleDto_ThrowsArgumentNullException()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
            var reader = new CsvObjectReader<UserDto>(stream);

            Assert.Throws<InvalidOperationException>(() => reader.ReadObject());
        }

        [Test]
        public void WithNullDictionaryAsRef_ThrowsArgumentNullException()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
            using var reader = new CsvDictionaryReader(stream);
            reader.AddMappings(Map);

            Assert.Throws<ArgumentNullException>(() => reader.ReadInto(null));
        }

        [Test]
        public void WithInvalidTempFile_ThrowsEndOfStreamException()
        {
            var tempFile = Path.GetTempFileName();
            var reader = new CsvObjectReader<UserDto>(tempFile);

            Assert.Throws<EndOfStreamException>(() => reader.ReadObject());
        }

        [Test]
        public void WithNoReadHeadingsCall_ThrowsInvalidOperationException()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
            var reader = new CsvObjectReader<UserDto>(stream);
            Assert.Throws<InvalidOperationException>(() => reader.ReadObject());
        }
    }

    public class Count : CsvReaderTest
    {
        [Test]
        public void WithValidStream_ReturnsNumberOfLines()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
            var reader = new CsvDictionaryReader(stream);

            reader.ReadHeadings();

            Assert.AreEqual(1, reader.Count);
        }
    }

    public class EscapeCharacter : CsvReaderTest
    {
        [Test]
        public void WithValidStream_GetsAndSetsSeparatorEscapeCharacter()
        {
            var reader = new CsvReader(new MemoryStream(), escapeChar: '?');

            Assert.AreEqual('?', reader.EscapeChar);
        }
    }

    public class SeparatorCharacter : CsvReaderTest
    {
        [Test]
        public void WithValidStream_GetsAndSetsSeparatorCharacter()
        {
            using var reader = new CsvReader(new MemoryStream(), separatorChar: '+');

            Assert.AreEqual('+', reader.SeparatorChar);
        }
    }

    public class Dispose : CsvReaderTest
    {
        [Test]
        public void WithDisposeAlreadyCalled_SetsHasDisposeToTrue()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
            var reader = new CsvDictionaryReader(stream);
            reader.ReadHeadings();

            var readObj = reader.ReadObject();
            reader.Dispose();
            reader.Dispose();

            Assert.IsNotNull(readObj);
        }
    }
}