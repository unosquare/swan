using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unosquare.Swan.Formatters;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class CsvWriterTest
    {
        private readonly string[] headers = new string[] { "Company", "OpenPositions", "MainTechnology", "Revenue" };
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
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvWriter(stream);
                
                Assert.Throws<ArgumentNullException>(() => {
                    reader.WriteObject(null);
                });
            }
        }

        [Test]
        public void WriteObjectDynamicObjectTest()
        {
            dynamic item = new ExpandoObject();
            item.A = "A";
            item.B = "B";
            item.C = "C";            
            var count = 0;

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvWriter(stream);
                reader.WriteObject(item);

                Assert.AreNotEqual(count, reader.Count);
            }
        }

        [Test]
        public void WriteObjectDictionaryTest()
        {
            Dictionary<string, object> item = new Dictionary<string, object>();
            item.Add("A", new { A = "A" });
            item.Add("B", new { B = "B" });
            item.Add("C", new { C = "C" });
            var count = 0;

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
            {
                var reader = new CsvWriter(stream);
                reader.WriteObject(item);

                Assert.AreNotEqual(count, reader.Count);
            }
        }
    }
}
