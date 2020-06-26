namespace Swan.Test.CsvWriterTest
{
    using Formatters;
    using Mocks;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

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
                writer.WriteHeadings(data);
                writer.WriteObjects(new List<object> { data.Select(k => k.Key) });
            }

            var valuesInFile = CsvReader.LoadRecords<object>(tempFile);
            Assert.AreEqual(1, valuesInFile.Count, "Same length");
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
            var generatedRecords = SampleCsvRecord.CreateSampleSet(TotalRows);
            generatedRecords.Add(null);

            Assert.Throws<ArgumentNullException>(() => CsvWriter.SaveRecords(generatedRecords, new MemoryStream()));
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
            reader.WriteObject(DefaultDictionary);

            Assert.AreNotEqual(0, reader.Count);
        }

        [Test]
        public void Array_ReturnsAreNotEqual()
        {
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
            var reader = new CsvWriter(stream);
            reader.WriteObject(DefaultStringList.ToArray());

            Assert.AreNotEqual(0, reader.Count);
        }

        [Test]
        public void Strings_ReturnsAreEqual()
        {
            var strings = SampleCsvRecord.SampleStringList();

            using var stream = new MemoryStream();
            using var writer = new CsvWriter(stream);
            writer.WriteObjects(strings);

            Assert.AreEqual((int)writer.Count, strings.Count);
        }

        [Test]
        public void DynamicObject_ReturnsAreEqual()
        {
            dynamic dynObject = new System.Dynamic.ExpandoObject();
            dynObject.A = nameof(MemoryStream);

            using var stream = new MemoryStream();
            using var writer = new CsvWriter(stream);
            writer.WriteObject(dynObject);

            Assert.IsNotNull(writer);
            Assert.AreEqual(1, (int)writer.Count);
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

            Assert.Throws<ArgumentNullException>(() => writer.WriteHeadings(NullType));
        }

        [Test]
        public void NullDictionary_ThrowsArgumentNullException()
        {
            using var stream = new MemoryStream();
            using var writer = new CsvWriter(stream);

            Assert.Throws<ArgumentNullException>(() =>
                    writer.WriteHeadings(null as Dictionary<string, string>));
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

            writer.WriteHeadings(dictionaryHeaders);

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

            var stringHeadersOutput = string.Join(",", stringHeaders.Select(x => x.Humanize()));

            using var stream = new MemoryStream();
            using var writer = new CsvWriter(stream);

            writer.WriteHeadings<SampleCsvRecord>();

            stream.Position = 0;
            var sr = new StreamReader(stream);
            var value = sr.ReadToEnd();
            var values = value.Split(',');

            Assert.AreEqual(stringHeadersOutput, value.Trim());
            Assert.AreEqual(stringHeaders.Length, values.Length);
        }

        [Test]
        public void WriteHeadingNull()
        {
            using var stream = new MemoryStream();
            using var writer = new CsvWriter(stream);

            Assert.Throws<ArgumentNullException>(
                () => writer.WriteHeadings(null as object));
        }

        [Test]
        public void WriteHeadingObject()
        {
            var stringHeaders = new[]
            {
                "Id", "AlternateId", "Name", "Description", "IsValidated", "ValidationResult", "Score", "CreationDate",
                "AccessDate",
            };

            var stringHeadersOutput = string.Join(",", stringHeaders.Select(x => x.Humanize()));

            var objHeaders = new SampleCsvRecord();

            using var stream = new MemoryStream();
            using var writer = new CsvWriter(stream);

            writer.WriteHeadings(objHeaders);

            stream.Position = 0;
            var sr = new StreamReader(stream);
            var value = sr.ReadToEnd();
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

            var stringHeadersOutput = string.Join("#", stringHeaders.Select(x => x.Humanize()));

            var objHeaders = new SampleCsvRecord();

            using var stream = new MemoryStream();
            using var writer = new CsvWriter(stream) {SeparatorCharacter = '#'};

            writer.WriteHeadings(objHeaders);

            stream.Position = 0;
            var sr = new StreamReader(stream);
            var value = sr.ReadToEnd();
            var values = value.Split('#');

            Assert.AreEqual(stringHeadersOutput, value.Trim());
            Assert.AreEqual(stringHeaders.Length, values.Length);
        }
    }
}
