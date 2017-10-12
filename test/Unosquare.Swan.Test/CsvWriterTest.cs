using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class CsvWriterTest
    {
        private const int TotalRows = 100;       
        private string _data = @"Company,OpenPositions,MainTechnology,Revenue
Co,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 "" 
Ca,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 """;

        [Test]
        public void ConstructorTest()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvWriter(stream, Encoding.ASCII);
                Assert.IsNotNull(reader);
            }
        }
        [Test]
        public void ConstructorEncodingNullTest()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvWriter(stream);
                Assert.IsNotNull(reader);
            }
        }

        [Test]
        public void ConstructorTempFileTest()
        {
            var tempFile = Path.GetTempFileName();
            var reader = new CsvWriter(tempFile);

            Assert.IsNotNull(reader);
        }

        [Test]
        public void ConstructorTempFileEncodingTest()
        {
            var tempFile = Path.GetTempFileName();
            var reader = new CsvWriter(tempFile, Encoding.ASCII);

            Assert.IsNotNull(reader);
        }

        [Test]
        public void WriteObjectTest()
        {
            var tempFile = Path.GetTempFileName();
            List<SampleCsvRecord> _generatedRecords = SampleCsvRecord.CreateSampleSet(TotalRows);

            CsvWriter.SaveRecords(_generatedRecords, tempFile);
            var loadedRecords = CsvReader.LoadRecords<SampleCsvRecord>(tempFile); 

            _generatedRecords.Add(null);

            Assert.Throws<ArgumentNullException>(() => {                
                CsvWriter.SaveRecords(_generatedRecords, tempFile);
            });
        }

        [Test]
        public void WriteObjectDynamicObjectTest()
        {
            var tempFile = Path.GetTempFileName();
            List<SampleCsvRecord> _generatedRecords = SampleCsvRecord.CreateSampleSet(TotalRows);

            CsvWriter.SaveRecords(_generatedRecords, tempFile);
            var loadedRecords = CsvReader.LoadRecords<SampleCsvRecord>(tempFile);

            dynamic item = SampleCsvRecord.GetItem();     

            _generatedRecords.Add(item);

            CsvWriter.SaveRecords(_generatedRecords, tempFile);
            var newloadedRecords = CsvReader.LoadRecords<SampleCsvRecord>(tempFile);

            Assert.AreNotEqual(loadedRecords, newloadedRecords);
        }

        [Test]
        public void WriteObjectDictionaryTest()
        {
            Dictionary<string, string> item = new Dictionary<string, string>();
            item.Add("A", "A");
            item.Add("B", "B");
            item.Add("C", "C");
            var count = 0;

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvWriter(stream);
                reader.WriteObject(item);

                Assert.AreNotEqual(count, reader.Count);
            }
        }

        [Test]
        public void WriteObjectArrayTest()
        {
            string[] item = new string[] { "A", "B", "C"};
            var count = 0;

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvWriter(stream);
                reader.WriteObject(item);

                Assert.AreNotEqual(count, reader.Count);
            }
        }

        [Test]
        public void WriteObjectTTest()
        {
            var tempFile = Path.GetTempFileName();
            List<SampleCsvRecord> _generatedRecords = SampleCsvRecord.CreateSampleSet(TotalRows);

            CsvWriter.SaveRecords(_generatedRecords, tempFile);
         
            var loadedRecords = CsvReader.LoadRecords<SampleCsvRecord>(tempFile);

            var item = SampleCsvRecord.GetItem();
            _generatedRecords.Add(item);

            CsvWriter.SaveRecords(_generatedRecords, tempFile);
            var newloadedRecords = CsvReader.LoadRecords<SampleCsvRecord>(tempFile);

            Assert.AreNotEqual(loadedRecords, newloadedRecords);
        }

        [Test]
        public void WriteHeadingsTest()
        {
            var tempFile = Path.GetTempFileName();            
            string[] headers = new string[] { "AccessDate", "AlternateId", "CreationDate", "Description", "Id", "IsValidated", "Name", "Score", "ValidationResult" };           
                        
            using (var stream = File.OpenWrite(tempFile))
            {
                if (stream.Length > 0)
                    stream.SetLength(0);

                using (var writer = new CsvWriter(stream))
                {
                    writer.WriteHeadings<SampleCsvRecord>();
                    writer.WriteObjects(headers);
                }                               
            }
            var loadedRecords = CsvReader.LoadRecords<SampleCsvRecord>(tempFile);

            Assert.AreEqual(headers.Length, loadedRecords.Count());     
       }

        [Test]
        public void WriteStringsTest()
        {
            int rows = 0;
            var tempFile = Path.GetTempFileName();
            var strings = SampleCsvRecord.SampleStringList();

            using (var stream = File.OpenWrite(tempFile))
            {
                if (stream.Length > 0)
                    stream.SetLength(0);

                using (var writer = new CsvWriter(stream))
                {
                    writer.WriteObjects(strings);
                    rows = (int)writer.Count;
                }

                Assert.AreEqual(rows, strings.Count);
            }
        }
    }
}
