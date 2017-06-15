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
        private readonly string[] headers = new string[] { "Id", "AlternateId", "Name", "Description", "IsValidated", "ValidationResult", "Score", "CreationDate", "AccessDate" };
        
        [Test]
        public void ConstructorTest()
        {
            using (var stream = SampleCsvRecord.GenerateStreamFromString("a,b \n c,d"))
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
            using (var stream = SampleCsvRecord.GenerateStreamFromString("a,b \n c,d"))
            {
                Assert.Throws<NullReferenceException>(() => {
                    var encodingNull = new CsvReader(stream, true, null);
                });
            }
        }

        [Test]
        public void ReadLineTest()
        {
            using (var stream = SampleCsvRecord.GenerateStreamFromString(string.Join(",", headers)))
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
            var tempFile = Path.GetTempFileName();
            var position = 0;

            var savedRecordCount = CsvWriter.SaveRecords(_generatedRecords, tempFile);
            var savedData = File.ReadAllLines(tempFile);

            using (var stream = SampleCsvRecord.GenerateStreamFromString(savedData[1]))
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
            using (var stream = SampleCsvRecord.GenerateStreamFromString(string.Join(",", headers)))
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
            using (var stream = SampleCsvRecord.GenerateStreamFromString(string.Join(",", headers)))
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
            var tempFile = Path.GetTempFileName();
            var savedRecordCount = CsvWriter.SaveRecords(_generatedRecords, tempFile);
            var loadedRecords = CsvReader.LoadRecords<SampleCsvRecord>(tempFile);

            using (var stream = SampleCsvRecord.GenerateStreamFromList(loadedRecords))
            {
                var reader = new CsvReader(stream);
                var headings = reader.ReadHeadings();
                Assert.Throws<EndOfStreamException>(() => {
                    var readObj = reader.ReadObject();
                });                
            }
        }

        [Test]
        public void QuotedTextTest()
        {
            var data =
                @"Company,OpenPositions,MainTechnology,Revenue
Co,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 "" 
Ca,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 """;

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(data)))
            {
                var reader =  new CsvReader(stream);
                var headers = reader.ReadHeadings();
                var firstLine = reader.ReadObject<SampleDto>();
            }
        }
    }
}
