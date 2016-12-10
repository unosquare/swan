using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unosquare.Swan;

namespace Unosquare.Swan.Samples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TestApplicationInfo();
            //TestTerminalOutputs();
            TestCsvFormatters();
            "Enter any key to exit".ReadKey();
        }

        static void TestApplicationInfo()
        {
            $"Operating System Type: {CurrentApp.OS}    CLR Type: {(CurrentApp.IsUsingMonoRuntime ? "Mono" : ".NET")}".Info();
            $"Local Storage Path: {CurrentApp.LocalStoragePath}".Info();
            $"Process Id: {CurrentApp.Process.Id}".Info();
        }

        static void TestTerminalOutputs()
        {
            ConsoleKeyInfo key = default(ConsoleKeyInfo);

            if ((key = Terminal.ReadKey("Press a key to output the current codepage. (X) will exit.")).Key == ConsoleKey.X) return;
            Terminal.WriteLine("CODEPAGE TEST", ConsoleColor.Blue);
            Terminal.PrintCurrentCodePage();

            if ((key = Terminal.ReadKey("Press a key to test logging output. (X) will exit.")).Key == ConsoleKey.X) return;
            Terminal.WriteLine("OUTPUT LOGGING TEST", ConsoleColor.Blue);
            $"This is some error".Error();
            $"This is some error".Error(nameof(TestTerminalOutputs));
            $"This is some info".Info();
            $"This is some info".Info(nameof(TestTerminalOutputs));
            $"This is some warning".Warn();
            $"This is some warning".Warn(nameof(TestTerminalOutputs));
            $"This is some tracing info".Trace();
            $"This is some tracing info".Trace(nameof(TestTerminalOutputs));
            $"This is for debugging stuff".Debug();
            $"This is for debugging stuff".Debug(nameof(TestTerminalOutputs));

            if ((key = Terminal.ReadKey("Press a key to test menu options. (X) will exit.")).Key == ConsoleKey.X) return;
            Terminal.WriteLine("TESTING MENU OPTIONS", ConsoleColor.Blue);

            Dictionary<ConsoleKey, string> SampleOptions = new Dictionary<ConsoleKey, string>
            {
                { ConsoleKey.A, "Sample A" },
                { ConsoleKey.B, "Sample B" }
            };

            "Please provide an option".ReadPrompt(SampleOptions, "Exit this program");

        }

        static void TestCsvFormatters()
        {

            var records = SampleCsvRecord.CreateSampleSet(100);
            var writeTestFilename = "WriterTest.csv"; ;
            var rewriteTestFilename = "RewriterTest.csv";

            var writeTestFilePath = CurrentApp.GetDesktopFilePath(writeTestFilename);
            var rewriteTestFilePath = CurrentApp.GetDesktopFilePath(rewriteTestFilename);

            if (File.Exists(writeTestFilePath))
                File.Delete(writeTestFilePath);

            if (File.Exists(rewriteTestFilePath))
                File.Delete(rewriteTestFilePath);

            using (var stream = File.OpenWrite(writeTestFilePath))
            {
                var writer = new Formatters.CsvWriter(stream, Constants.Windows1252Encoding);
                writer.WriteHeadings<SampleCsvRecord>();
                writer.WriteObjects(records);
            }

            var recordsWithNewLines = records.Where(s => s.Description.Contains("\r") || s.Description.Contains("\r")).ToArray();
            $"Records (a total of {recordsWithNewLines.Length}) {string.Join(", ", recordsWithNewLines.Select(r => r.Id))} have a newline sequence in the description".Trace(nameof(TestCsvFormatters));

            var parsedRecords = new List<SampleCsvRecord>();
            using (var reader = new Formatters.CsvReader(writeTestFilePath))
            {
                reader.ReadHeadings();
                while (reader.EndOfStream == false)
                {
                    var record = reader.ReadObject<SampleCsvRecord>();
                    parsedRecords.Add(record);
                }
            }

            $"Parsed a total of {parsedRecords.Count} records".Trace(nameof(TestCsvFormatters));

            using (var stream = File.OpenWrite(rewriteTestFilePath))
            {
                var writer = new Formatters.CsvWriter(stream, Constants.Windows1252Encoding);
                writer.WriteHeadings<SampleCsvRecord>();
                writer.WriteObjects(parsedRecords);
            }
        }
    }

    internal class SampleCsvRecord
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
            "Hello, this is a test of the beautifu SWAN linbrary. \r \r \r \r "
            + "It is helpful because it contains some easy to use code and stuff that is handy at all thimes. \r\n \r\n \r\n \r\n \r\n  "
            + "Swan is free to use and it is MIT licensed. It is a collection of patterns and helpful classes that make it super easy to code complex stuff \n "
            + "For example the AppWorker class allows you to write threaded background services and catch start and stop events. "
            + "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum. "
            + "Provides methods for creating, manipulating, searching, and sorting arrays, thereby serving as the base class for all arrays in the common language runtime. "
            + "The CSV formatters anllow you to quickly and easily read to and from CSV files.  \r \r \r \r \r  "
            + "\n \n \n \n \n \n \n \n \n \n \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \"quoted\""
            + "It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout. The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English. Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy. Various versions have evolved over the years, sometimes by accident, sometimes on purpose injected humour and the like."
            + "SWAN also provides helpful extension methods for string manipulation").Split(new string[] { " " }, StringSplitOptions.None);

        static public List<SampleCsvRecord> CreateSampleSet(int size)
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
                    IsValidated = random.NextDouble() > 0.5d ? true : false,
                    Name = RandomWords[random.Next(0, RandomWords.Length - 1)],
                    Score = Convert.ToSingle(random.NextDouble() * random.Next(10, 1000)),
                    ValidationResult = random.NextDouble() > 0.5d ? true : false
                };

                result.Add(record);
            }

            return result;
        }

    }
}
