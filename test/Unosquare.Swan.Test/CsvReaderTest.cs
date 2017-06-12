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

        [Test]
        public void ConstructorTest()
        {
            var tempFile = Path.GetTempFileName();
                   
            using (var stream = SampleCsvRecord.GenerateStreamFromString("a,b \n c,d"))
            {
                var reader = new CsvReader(stream, true, Encoding.ASCII);
                Assert.IsNotNull(reader);
            }
        }

        [Test]
        public void ConstructorStreamNull()
        {
            using (var stream = SampleCsvRecord.GenerateStreamFromString("a,b \n c,d"))
            {
                Assert.Throws<NullReferenceException>(() => {
                    var streamNull = new CsvReader(null, true, Encoding.ASCII);
                });
            }           
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
        public void ReadHedings()
        {
            var tempFile = Path.GetTempFileName();
            var headers = new String[] { "Id", "AlternateId", "Name", "Description", "IsValidated", "ValidationResult", "Score", "CreationDate", "AccessDate" };

            using (var stream = SampleCsvRecord.GenerateStreamFromString(string.Join("",headers)))
            {
                var reader = new CsvReader(tempFile);
                var headings = reader.ReadHeadings();

                Assert.IsNotNull(headings);
                Assert.AreEqual(headers, headings);
            }
        }
    }
}
