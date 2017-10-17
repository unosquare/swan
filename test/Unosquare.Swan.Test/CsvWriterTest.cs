﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        [TestFixture]
        public class Constructor : CsvWriterTest
        {
            [Test]
            public void WithMemoryStreamAndEncoding_Valid()
            {
                using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
                {
                    var reader = new CsvWriter(stream, Encoding.ASCII);
                    Assert.IsNotNull(reader);
                }
            }

            [Test]
            public void WithMemoryStream_Valid()
            {
                using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
                {
                    var reader = new CsvWriter(stream);
                    Assert.IsNotNull(reader);
                }
            }

            [Test]
            public void WithTempFile_Valid()
            {
                var tempFile = Path.GetTempFileName();
                var reader = new CsvWriter(tempFile);

                Assert.IsNotNull(reader);
            }

            [Test]
            public void WithTempFileAndEncoding_Valid()
            {
                var tempFile = Path.GetTempFileName();
                var reader = new CsvWriter(tempFile, Encoding.ASCII);

                Assert.IsNotNull(reader);
            }
        }
        
        [TestFixture]
        public class SaveRecords : CsvWriterTest
        {
            [Test]
            public void WithObjectList_Valid()
            {
                var tempFile = Path.GetTempFileName();
                var generatedRecords = SampleCsvRecord.CreateSampleSet(TotalRows);

                CsvWriter.SaveRecords(generatedRecords, tempFile);

                var valuesInFile = CsvReader.LoadRecords<SampleCsvRecord>(tempFile);
                Assert.AreEqual(generatedRecords.Count, valuesInFile.Count, "Same length");
                Assert.AreEqual(generatedRecords[0].Name, valuesInFile[0].Name, "Same first name");
            }


            [Test]
            public void WithNullList_Invalid()
            {
                var tempFile = Path.GetTempFileName();
                var generatedRecords = SampleCsvRecord.CreateSampleSet(TotalRows);
                generatedRecords.Add(null);

                Assert.Throws<ArgumentNullException>(() =>
                {
                    CsvWriter.SaveRecords(generatedRecords, tempFile);
                });
            }
        }

        [TestFixture]
        public class WriteObject : CsvWriterTest
        {
            // TODO: Continue
        }

        [TestFixture]
        public class WriteHeadings : CsvWriterTest
        {
            // TODO: Continue
        }
        
        [Test]
        public void WriteObjectDictionaryTest()
        {
            var item = new Dictionary<string, string> {{"A", "A"}, {"B", "B"}, {"C", "C"}};

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvWriter(stream);
                reader.WriteObject(item);

                Assert.AreNotEqual(0, reader.Count);
            }
        }

        [Test]
        public void WriteObjectArrayTest()
        {
            var item = new[] {"A", "B", "C"};

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvWriter(stream);
                reader.WriteObject(item);

                Assert.AreNotEqual(0, reader.Count);
            }
        }
        
        [Test]
        public void WriteHeadingsTest()
        {
            var tempFile = Path.GetTempFileName();
            var headers = new[]
            {
                "AccessDate", "AlternateId", "CreationDate", "Description", "Id", "IsValidated", "Name", "Score",
                "ValidationResult"
            };

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
            var strings = SampleCsvRecord.SampleStringList();

            using (var stream = new MemoryStream())
            {
                using (var writer = new CsvWriter(stream))
                {
                    writer.WriteObjects(strings);

                    Assert.AreEqual((int) writer.Count, strings.Count);
                }
            }
        }
    }
}