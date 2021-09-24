using NUnit.Framework;
using Swan.Formatters;
using Swan.Test.Mocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Swan.Test.CsvWriterTest
{
    public abstract class CsvWriterTest : TestFixtureBase
    {
        protected const int TotalRows = 100;

        protected string Data = @"Company,OpenPositions,MainTechnology,Revenue
                Co,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 "" 
                Ca,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 """;
    }

    [TestFixture]
    public class Constructor : CsvWriterTest
    {
        [Test]
        public void WithMemoryStreamAndEncoding_Valid()
        {
            using var stream = new MemoryStream();
            var reader = new CsvWriter(stream, Encoding.ASCII);
            Assert.IsNotNull(reader);
        }

        [Test]
        public void WithMemoryStream_Valid()
        {
            using var stream = new MemoryStream();
            var reader = new CsvWriter(stream);
            Assert.IsNotNull(reader);
        }

        [Test]
        public void WithTempFile_Valid()
        {
            var tempFile = Path.GetTempFileName();
            using var fs = File.OpenWrite(tempFile);
            var reader = new CsvWriter(fs);

            Assert.IsNotNull(reader);
        }

        [Test]
        public void WithTempFileAndEncoding_Valid()
        {
            var tempFile = Path.GetTempFileName();
            using var fs = File.OpenWrite(tempFile);
            var reader = new CsvWriter(fs, Encoding.ASCII);

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

            var data = new Dictionary<string, string>
            {
                {"AccessDate", "20171107"},
                {"AlternateId", "1"},
                {"CreationDate", "20171107"},
                {"Description", "Sr. Software Engineer"},
                {"Id", "0001"},
                {"IsValidated", "true"},
                {"Name", "Alexey Turpalov"},
                {"Score", "1245F"},
                {"ValidationResult", "true"},
            };

            using (var stream = File.OpenWrite(tempFile))
            {
                using var writer = new CsvWriter(stream);
                writer.WriteLine(data.Keys.AsEnumerable());
                writer.WriteLine(data.Values.AsEnumerable());
            }

            var valuesInFile = Csv.Load(tempFile);
            Assert.AreEqual(1, valuesInFile.Count, "Same length");
        }

        [Test]
        public void WithObjectList_Valid()
        {
            var tempFile = Path.GetTempFileName();
            var generatedRecords = SampleCsvRecord.CreateSampleSet(TotalRows);

            Csv.Save(generatedRecords, tempFile);

            var valuesInFile = Csv.Load<SampleCsvRecord>(tempFile);
            Assert.AreEqual(generatedRecords.Count, valuesInFile.Count, "Same length");
            Assert.AreEqual(generatedRecords[0].Name, valuesInFile[0].Name, "Same first name");
        }

        [Test]
        public void WithNullList_Invalid()
        {
            var generatedRecords = SampleCsvRecord.CreateSampleSet(TotalRows);
            generatedRecords.Add(null);

            Assert.Throws<ArgumentNullException>(() => Csv.Save(generatedRecords, new MemoryStream()));
        }
    }

    [TestFixture]
    public class WriteObject : CsvWriterTest
    {
        [Test]
        public void Dictionary_ReturnsAreNotEqual()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
            var reader = new CsvWriter(stream);
            reader.WriteLine(DefaultDictionary.Values);

            Assert.AreNotEqual(0, reader.Count);
        }

        [Test]
        public void Array_ReturnsAreNotEqual()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
            var reader = new CsvWriter(stream);
            reader.WriteLine(DefaultStringList.ToArray());

            Assert.AreNotEqual(0, reader.Count);
        }

        [Test]
        public void Strings_ReturnsAreEqual()
        {
            var strings = SampleCsvRecord.SampleStringList();

            using var stream = new MemoryStream();
            using var writer = new CsvWriter(stream);
            foreach (var s in strings)
                writer.WriteLine(s);

            Assert.AreEqual((int)writer.Count, strings.Count);
        }

        [Test]
        public void DynamicObject_ReturnsAreEqual()
        {
            dynamic dynObject = new System.Dynamic.ExpandoObject();
            dynObject.A = nameof(MemoryStream);

            using var stream = new MemoryStream();
            using var writer = new CsvWriter<dynamic>(stream);
            writer.WriteLine(dynObject);

            Assert.IsNotNull(writer);
            Assert.AreEqual(2, (int)writer.Count);
        }
    }

    [TestFixture]
    public class WriteHeadings : CsvWriterTest
    {
        [Test]
        public void NullType_ThrowsArgumentNullException()
        {
            using var stream = new MemoryStream();
            using var writer = new CsvWriter(stream);

            Assert.Throws<ArgumentNullException>(() => writer.WriteLine(NullType as IEnumerable<string>));
        }

        [Test]
        public void WritingHeadersFromDictionary_WritesHeaders()
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
                {"ValidationResult", "true"},
            };

            var stringHeaders = dictionaryHeaders.Select(k => k.Key).ToList();

            using var stream = new MemoryStream();
            using var writer = new CsvWriter(stream);

            var stringHeadersOutput = string.Join(",", stringHeaders) + writer.NewLineSequence;

            writer.WriteLine(dictionaryHeaders.Keys);
            writer.Flush();

            stream.Position = 0;
            var sr = new StreamReader(stream);
            var value = sr.ReadToEnd();
            var values = value.Split(',');

            Assert.AreEqual(stringHeadersOutput, value);
            Assert.AreEqual(stringHeaders.Count, values.Length);
        }

        [Test]
        public void WritingHeadersFromSampleClass_WritesHeaders()
        {
            var stringHeaders = new[]
            {
                "Id", "AlternateId", "Name", "Description", "IsValidated", "ValidationResult", "Score", "CreationDate",
                "AccessDate",
            };

            var stringHeadersOutput = string.Join(",", stringHeaders);

            using var stream = new MemoryStream();
            using var writer = new CsvWriter<SampleCsvRecord>(stream);

            writer.WriteLine(new());
            writer.Flush();

            stream.Position = 0;
            var sr = new StreamReader(stream);
            var value = sr.ReadLine();
            var values = value.Split(',');

            Assert.AreEqual(stringHeadersOutput, value.Trim());
            Assert.AreEqual(stringHeaders.Length, values.Length);
        }

        [Test]
        public void WriteHeadingNull()
        {
            using var stream = new MemoryStream();
            using var writer = new CsvWriter<object>(stream);

            Assert.Throws<ArgumentNullException>(
                () => writer.WriteLine(null));
        }

        [Test]
        public void WriteHeadingObject()
        {
            var stringHeaders = new[]
            {
                "Id", "AlternateId", "Name", "Description", "IsValidated", "ValidationResult", "Score", "CreationDate",
                "AccessDate",
            };

            var stringHeadersOutput = string.Join(",", stringHeaders);

            var objHeaders = new SampleCsvRecord();

            using var stream = new MemoryStream();
            using var writer = new CsvWriter<SampleCsvRecord>(stream);

            writer.WriteLine(objHeaders);
            writer.Flush();

            stream.Position = 0;
            var sr = new StreamReader(stream);
            var value = sr.ReadLine();
            var values = value.Split(',');

            Assert.AreEqual(stringHeadersOutput, value.Trim());
            Assert.AreEqual(stringHeaders.Length, values.Length);
        }

        [Test]
        public void ChangeSeparator()
        {
            var stringHeaders = new[]
            {
                "Id", "AlternateId", "Name", "Description", "IsValidated", "ValidationResult", "Score", "CreationDate",
                "AccessDate",
            };

            var stringHeadersOutput = string.Join("#", stringHeaders);

            var objHeaders = new SampleCsvRecord();

            using var stream = new MemoryStream();
            using var writer = new CsvWriter<SampleCsvRecord>(stream, separatorChar: '#');

            writer.WriteLine(objHeaders);
            writer.Flush();
            stream.Position = 0;
            var sr = new StreamReader(stream);
            var value = sr.ReadLine();
            var values = value.Split('#');

            Assert.AreEqual(stringHeadersOutput, value.Trim());
            Assert.AreEqual(stringHeaders.Length, values.Length);
        }
    }
}