﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Test.Mocks;
using System.Linq;

namespace Unosquare.Swan.Test.CsvWriterTest
{
    public abstract class CsvWriterTest
    {
        protected const int TotalRows = 100;

        protected string _data = @"Company,OpenPositions,MainTechnology,Revenue
                Co,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 "" 
                Ca,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 """;
    }

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
        public void TempFileFilled_SetStreamLengthToZero()
        {
            var tempFile = Path.GetTempFileName();
            var generatedRecords = SampleCsvRecord.CreateSampleSet(TotalRows);
            var stringHeaders = new List<string>();
            var dictionaryheaders = new Dictionary<string, string>
            {
                {"AccessDate", "20171107"},
                {"AlternateId", "1"},
                {"CreationDate", "20171107"},
                {"Description", "Sr. Software Engineer"},
                {"Id", "0001"},
                {"IsValidated", "true"},
                {"Name", "Alexey Turpalov"},
                {"Score", "1245F"},
                {"ValidationResult", "true"}
            };

            stringHeaders = dictionaryheaders.Select(k => k.Key).ToList();

            using (var stream = File.OpenWrite(tempFile))
            {
                using (var writer = new CsvWriter(stream))
                {
                    writer.WriteHeadings(dictionaryheaders);
                    writer.WriteObjects(stringHeaders);
                }
            }

            CsvWriter.SaveRecords(generatedRecords, tempFile);

            var valuesInFile = CsvReader.LoadRecords<SampleCsvRecord>(tempFile);
            Assert.AreEqual(generatedRecords.Count, valuesInFile.Count, "Same length");
        }

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
        [Test]
        public void Dictionary_ReturnsAreNotEqual()
        {
            var item = new Dictionary<string, string> { { "A", "A" }, { "B", "B" }, { "C", "C" } };

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvWriter(stream);
                reader.WriteObject(item);

                Assert.AreNotEqual(0, reader.Count);
            }
        }

        [Test]
        public void Array_ReturnsAreNotEqual()
        {
            var item = new[] { "A", "B", "C" };

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvWriter(stream);
                reader.WriteObject(item);

                Assert.AreNotEqual(0, reader.Count);
            }
        }

        [Test]
        public void Strings_ReturnsAreEqual()
        {
            var strings = SampleCsvRecord.SampleStringList();

            using (var stream = new MemoryStream())
            {
                using (var writer = new CsvWriter(stream))
                {
                    writer.WriteObjects(strings);

                    Assert.AreEqual((int)writer.Count, strings.Count);
                }
            }
        }

        [Test]
        public void DynamicObject_ReturnsAreEqual()
        {
            dynamic dynObject = new Dictionary<string, object>
            {
                {"A", new { Name = "Florencia" }},
                {"B", new { Name = "Camila" }},
                {"C", new { Name = "Florencia" }},
                {"D", new { Name = "Mónica" }}
            };

            using (var stream = new MemoryStream())
            {
                using (var writer = new CsvWriter(stream))
                {
                    writer.WriteObject(dynObject);

                    Assert.IsNotNull(writer);
                    Assert.AreEqual(1, (int)writer.Count);
                }
            }
        }
    }

    [TestFixture]
    public class WriteHeadings : CsvWriterTest
    {
        [Test]
        public void NullType_ThrowsArgumentNullException()
        {            
            using (var stream = new MemoryStream())
            {
                using (var writer = new CsvWriter(stream))
                {
                    Assert.Throws<ArgumentNullException>(() =>
                    {
                        writer.WriteHeadings(null as Type);
                    });
                }
            }
        }

        [Test]
        public void NullDictionary_ThrowsArgumentNullException()
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new CsvWriter(stream))
                {
                    Assert.Throws<ArgumentNullException>(() =>
                    {
                        writer.WriteHeadings(null as Dictionary<string, string>);
                    });
                }
            }
        }

        [Test]
        public void HeadersDictionaryLength_ReturnsAreEqual()
        {
            var dictionaryHeaders = new Dictionary<string, string>
            {
                {"AccessDate", "20171107"},
                {"AlternateId", "1"},
                {"CreationDate", "20171107"},
                {"Description", "Sr. Software Engineer"},
                {"Id", "0001"},
                {"IsValidated", "true"},
                {"Name", "Alexey Turpalov"},
                {"Score", "1245F"},
                {"ValidationResult", "true"}
            };
            var stringHeadersOutput = "AccessDate,AlternateId,CreationDate,Description,Id,IsValidated,Name,Score,ValidationResult\r\n";

            var stringHeaders = dictionaryHeaders.Select(k => k.Key).ToList();

            using (var stream = new MemoryStream())
            {
                using (var writer = new CsvWriter(stream))
                {
                    writer.WriteHeadings(dictionaryHeaders);

                    stream.Position = 0;
                    var sr = new StreamReader(stream);
                    var myStr = sr.ReadToEnd();
                    var myStrSplitted = myStr.Split(',');

                    Assert.AreEqual(stringHeadersOutput, myStr);
                    Assert.AreEqual(stringHeaders.Count, myStrSplitted.Length);
                }
            }
        }

        [Test]
        public void HeadersLength_ReturnsAreEqual()
        {
            var stringHeaders = new[]
            {
                "Id", "AlternateId", "Name", "Description", "IsValidated", "ValidationResult", "Score", "CreationDate",
                "AccessDate"
            };
            var stringHeadersOutput = string.Join(",", stringHeaders);

            using (var stream = new MemoryStream())
            {
                using (var writer = new CsvWriter(stream))
                {
                    writer.WriteHeadings<SampleCsvRecord>();

                    stream.Position = 0;
                    var sr = new StreamReader(stream);
                    var myStr = sr.ReadToEnd();
                    var myStrSplitted = myStr.Split(',');

                    Assert.AreEqual(stringHeadersOutput, myStr.Trim());
                    Assert.AreEqual(stringHeaders.Length, myStrSplitted.Length);
                }
            }
        }
    }
}