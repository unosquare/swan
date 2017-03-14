using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class CsvTest
    {
        private const int TotalRows = 100;
        private readonly List<SampleCsvRecord> _generatedRecords = SampleCsvRecord.CreateSampleSet(TotalRows);

        [Test]
        public void ReadWriteTest()
        {
            var tempFile = Path.GetTempFileName();

            var savedRecordCount = CsvWriter.SaveRecords(_generatedRecords, tempFile);

            // Minus one because SaveRecords includes header
            Assert.AreEqual(TotalRows, savedRecordCount - 1);

            var savedData = File.ReadAllLines(tempFile);
            Assert.IsNotNull(savedData);
            Assert.Greater(savedData.Length, 0);
            Assert.IsTrue(savedData[0].StartsWith("Id"));


            var loadedRecords = CsvReader.LoadRecords<SampleCsvRecord>(tempFile);

            Assert.IsNotNull(loadedRecords);
            Assert.AreEqual(TotalRows, loadedRecords.Count);

            var i = 0;

            foreach (var row in _generatedRecords)
                Assert.AreEqual(row.ToStringInvariant(), loadedRecords[i++].ToStringInvariant());
        }
    }
}
