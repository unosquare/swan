using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test.CsvReaderTest
{
    [TestFixture]
    public abstract class CsvReaderTest
    {
        protected readonly string[] _headers = {"Company", "OpenPositions", "MainTechnology", "Revenue"};

        protected readonly string _data = @"Company,OpenPositions,MainTechnology,Revenue
Co,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 "" 
Ca,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 """;
    }

    public class Constructor : CsvReaderTest
    {
        [Test]
        public void WithValidStreamAndValidEncoding_ReturnsReader()
        {
            using(var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream, true, Encoding.ASCII);
                
                Assert.IsNotNull(reader);
            }
        }

        [Test]
        public void WithNullStream_ThrowsNullReferenceException()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                var csvReader = new CsvReader(null, true, Encoding.ASCII);
            });
        }

        [Test]
        public void WithNullEncoding_ThrowsNullReferenceException()
        {
            using(var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                Assert.Throws<NullReferenceException>(() =>
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

            using(var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream, Encoding.ASCII);
                reader.SkipRecord();
                Assert.AreNotEqual(stream.Position, position);
            }
        }

        [Test]
        public void WithInvalidString_ThrowsEndOfStreamException()
        {
            var tempFile = Path.GetTempFileName();

            var reader = new CsvReader(tempFile);
            Assert.Throws<EndOfStreamException>(() =>
            {
                reader.SkipRecord();
            });
        }
    }

    public class ReadHeadings : CsvReaderTest
    {
        [Test]
        public void WithValidStream_ReturnsAnArray()
        {
            using(var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream);
                var headings = reader.ReadHeadings();
                Assert.IsNotNull(headings);
                Assert.AreEqual(_headers, headings);
            }
        }

        [Test]
        public void WithSameReader_ThrowsInvalidOperationException()
        {
            using(var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream);
                reader.ReadHeadings();
                
                Assert.Throws<InvalidOperationException>(() =>
                {
                    reader.ReadHeadings();
                });
            }
        }
    }

    public class ReadLine : CsvReaderTest
    {
        [Test]
        public void WithValidStream_ReturnsAnArray()
        {
            using(var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
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
            Assert.Throws<EndOfStreamException>(() =>
            {
                reader.ReadLine();
            });
        }
    }
    
    public class ReadObject : CsvReaderTest
    {
        [Test]
        public void WithValidStream_ReturnsADictionary()
        {
            using(var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
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
            using(var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream);

                Assert.Throws<InvalidOperationException>(() =>
                {
                    var readObj = reader.ReadObject();
                });
            }
        }
        
        [Test]
        public void WithEndOfStream_ThrowsEndOfStreamException()
        {
            using(var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream);
                reader.ReadHeadings();

                if(reader.EndOfStream)
                {
                    Assert.Throws<EndOfStreamException>(() =>
                    {
                        reader.ReadObject<SampleDto>();
                    });
                }
            }
        }

        [Test]
        public void WithDictionary_ThrowsInvalidOperationException()
        {
            var map = new Dictionary<string, string>
            {
                {"First", "Company"},
                {"Second", "Open Position"},
                {"Thrid", "Main Technology"}
            };

            using(var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream);
                Assert.Throws<InvalidOperationException>(() =>
                {
                    reader.ReadObject<SampleDto>(map);
                });
            }
        }

        [Test]
        public void ReadObjectArgumentNull()
        {
            using(var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream);
                reader.ReadHeadings();

                Assert.Throws<ArgumentNullException>(() =>
                {
                    var readObj = reader.ReadObject(null);
                });
            }
        }
        
        [Test]
        public void WithSampleDto_ThrowsArgumentNullException()
        {
            using(var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream);

                Assert.Throws<ArgumentNullException>(() =>
                {
                    reader.ReadObject<SampleDto>();
                });
            }
        }

    }
}