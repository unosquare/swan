using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Unosquare.Swan.Test.Mocks
{
    public class SampleCsvRecord
    {
        public int Id { get; set; }
        public int? AlternateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public bool IsValidated { get; set; }
        public bool? ValidationResult { get; set; }

        public float Score { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime? AccessDate { get; set; }

        private static readonly string[] RandomWords = (
            "Hello, this is a test of the beautiful SWAN library. \r \r \r \r "
            + "It is helpful because it contains some easy to use code and stuff that is handy at all times. \r\n \r\n \r\n \r\n \r\n  "
            + "Swan is free to use and it is MIT licensed. It is a collection of patterns and helpful classes that make it super easy to code complex stuff \n "
            + "For example the AppWorker class allows you to write threaded background services and catch start and stop events. "
            + "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum. "
            + "Provides methods for creating, manipulating, searching, and sorting arrays, thereby serving as the base class for all arrays in the common language runtime. "
            + "The CSV formatters allow you to quickly and easily read to and from CSV files.  \r \r \r \r \r  "
            + "\n \n \n \n \n \n \n \n \n \n \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \"quoted\""
            + "It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout. The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English. Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy. Various versions have evolved over the years, sometimes by accident, sometimes on purpose injected humour and the like."
            + "SWAN also provides helpful extension methods for string manipulation").Split(new[] { " " }, StringSplitOptions.None);

        public static List<SampleCsvRecord> CreateSampleSet(int size)
        {
            var result = new List<SampleCsvRecord>();
            var random = new Random();

            for (var i = 0; i < size; i++)
            {
                var descriptionLength = random.Next(5, RandomWords.Length);
                var descriptionSb = new StringBuilder();
                for (var wi = 0; wi < descriptionLength; wi++)
                {
                    descriptionSb.Append(
                        $"{RandomWords[random.Next(0, RandomWords.Length - 1)]} ");
                }

                var record = new SampleCsvRecord
                {
                    AccessDate = random.NextDouble() > 0.5d ? DateTime.Now : new DateTime?(),
                    AlternateId = random.NextDouble() > 0.5d ? random.Next(10, 9999999) : new int?(),
                    CreationDate = random.NextDouble() > 0.5d ? DateTime.Now : DateTime.MinValue,
                    Description = descriptionSb.ToString(),
                    Id = i,
                    IsValidated = random.NextDouble() > 0.5d,
                    Name = RandomWords[random.Next(0, RandomWords.Length - 1)],
                    Score = Convert.ToSingle(random.NextDouble() * random.Next(10, 1000)),
                    ValidationResult = random.NextDouble() > 0.5d
                };

                result.Add(record);
            }

            return result;
        }
        public static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }   
    }
}
