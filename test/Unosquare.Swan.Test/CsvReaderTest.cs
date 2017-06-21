using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class CsvReaderTest
    {
        private const int TotalRows = 100;
        private readonly List<SampleCsvRecord> _generatedRecords = SampleCsvRecord.CreateSampleSet(TotalRows);
        private readonly string[] headers = new string[] { "Company", "OpenPositions", "MainTechnology", "Revenue"};
        private string _data = @"Company,OpenPositions,MainTechnology,Revenue
Co,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 "" 
Ca,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 """;

        [Test]
        public void ConstructorTest()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream, true, Encoding.ASCII);
                Assert.IsNotNull(reader);
            }
        }

        [Test]
        public void ConstructorStreamNull()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                var csvReader = new CsvReader(null, true, Encoding.ASCII);
            });
        }

        [Test]
        public void ConstructorEncodingNull()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                Assert.Throws<NullReferenceException>(() => {
                    var encodingNull = new CsvReader(stream, true, null);
                });
            }
        }

        [Test]
        public void ReadLineTest()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream);
                var line = reader.ReadLine();
                Assert.IsNotEmpty(line);
            }
        }

        [Test]
        public void ReadLineTestEndOfStreamException()
        {
            var tempFile = Path.GetTempFileName();

            var reader = new CsvReader(tempFile);
            Assert.Throws<EndOfStreamException>(() => {
                reader.ReadLine();
            });                        
        }

        [Test]
        public void SkipRecordTest()
        {
            var position = 0;

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream, Encoding.ASCII);
                reader.SkipRecord();
                Assert.AreNotEqual(stream.Position, position);
            }
        }

        [Test]
        public void SkipRecordEndOfStreamException()
        {
            var tempFile = Path.GetTempFileName();

            var reader = new CsvReader(tempFile);
            Assert.Throws<EndOfStreamException>(() => {
                reader.SkipRecord();
            });
        }

        [Test]
        public void ReadHedingsTest()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream);
                var headings = reader.ReadHeadings();
                Assert.IsNotNull(headings);
                Assert.AreEqual(headers, headings);
            }
        }

        [Test]
        public void ReadHedingsInvalidOperation()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream);
                var headings = reader.ReadHeadings();

                Assert.IsNotNull(headings);
                Assert.AreEqual(headers, headings);

                Assert.Throws<InvalidOperationException>(() => {
                    reader.ReadHeadings();
                });
            }
        }

        [Test]
        public void ReadObjectTest()
        {            
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream);
                var headings = reader.ReadHeadings();
                var readObj = reader.ReadObject();

                Assert.IsNotNull(readObj);            
            }
        }

        [Test]
        public void ReadObjectInvalidOperation()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream);

                Assert.Throws<InvalidOperationException>(() => {
                    var readObj = reader.ReadObject();
                });
            }
        }

        [Test]
        public void ReadObjectArgumentNull()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream);
                var headings = reader.ReadHeadings();

                Assert.Throws<ArgumentNullException>(() => {
                    var readObj = reader.ReadObject(null);
                });
            }
        }

        [Test]
        public void QuotedTextTest()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader =  new CsvReader(stream);
                var headers = reader.ReadHeadings();
                var firstLine = reader.ReadObject<SampleDto>();
            }
        }

        [Test]
        public void ReadObjectTArgumentNull()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream);
                Assert.Throws<ArgumentNullException>(() => {
                    reader.ReadObject<SampleDto>();
                });                
            }
        }

        [Test]
        public void ReadObjectTInvalidOperation()
        {
            Dictionary<string,string> map = new Dictionary<string, string>();
            map.Add("First","Company");
            map.Add("Second", "Open Position");
            map.Add("Thrid", "Main Technology");

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream);
                Assert.Throws<InvalidOperationException>(() => {
                    reader.ReadObject<SampleDto>(map);
                });
            }
        }

        [Test]
        public void ReadObjectTEndOfStream()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvReader(stream);
                var headers = reader.ReadHeadings();
                if(reader.EndOfStream)
                    Assert.Throws<EndOfStreamException>(() => {
                        reader.ReadObject<SampleDto>();
                    });
            }            
        }
    }
}
