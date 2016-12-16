namespace Unosquare.Swan.Samples
{
    using Abstractions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Unosquare.Swan.Formatters;

    public class Program
    {
        public static void Main(string[] args)
        {
            TestSignleton.Instance.Name.Info("Singleton Test");

            TestApplicationInfo();
            TestTerminalOutputs();
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
            var action = new Action(() =>
            {
                var test01FilePath = CurrentApp.GetDesktopFilePath("csv-writer-test-01.csv");
                var test02FilePath = CurrentApp.GetDesktopFilePath("csv-witer-test-02.csv");

                var generatedRecords = SampleCsvRecord.CreateSampleSet(100);
                $"Generated {generatedRecords.Count} sample records.".Info(nameof(TestCsvFormatters));

                var savedRecordCount = CsvWriter.SaveRecords(generatedRecords, test01FilePath);
                $"Saved {savedRecordCount} records (including header) to file: {Path.GetFileName(test01FilePath)}.".Info(nameof(TestCsvFormatters));

                var loadedRecords = CsvReader.LoadRecords<SampleCsvRecord>(test01FilePath);
                $"Loaded {(loadedRecords.Count)} records from file: {Path.GetFileName(test01FilePath)}.".Info(nameof(TestCsvFormatters));

                savedRecordCount = CsvWriter.SaveRecords(generatedRecords, test02FilePath);
                $"Saved {savedRecordCount} records (including header) to file: {Path.GetFileName(test02FilePath)}.".Info(nameof(TestCsvFormatters));

                var sourceObject = loadedRecords[generatedRecords.Count / 2];
                var targetObject = new StubCopyTargetTest();
                var copiedProperties = sourceObject.CopyPropertiesTo(targetObject);
                $"{nameof(Extensions.CopyPropertiesTo)} method copied {copiedProperties} properties from one object to another".Info(nameof(TestCsvFormatters));
            });

            var elapsed = action.Benchmark();
            $"Elapsed: {Math.Round(elapsed.TotalMilliseconds, 3)} milliseconds".Trace();
        }
    }

    internal class TestSignleton : SingletonBase<TestSignleton>
    {
        private TestSignleton() { }
        public string Name => "Hello";
    }

    internal class StubCopyTargetTest
    {
        public float ID { get; set; }
        public decimal AlternateId { get; set; }
        public string Score { get; set; }
        public DateTime CreationDate { get; set; }
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
            "Hello, this is a test of the beautiful SWAN library. \r \r \r \r "
            + "It is helpful because it contains some easy to use code and stuff that is handy at all times. \r\n \r\n \r\n \r\n \r\n  "
            + "Swan is free to use and it is MIT licensed. It is a collection of patterns and helpful classes that make it super easy to code complex stuff \n "
            + "For example the AppWorker class allows you to write threaded background services and catch start and stop events. "
            + "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum. "
            + "Provides methods for creating, manipulating, searching, and sorting arrays, thereby serving as the base class for all arrays in the common language runtime. "
            + "The CSV formatters allow you to quickly and easily read to and from CSV files.  \r \r \r \r \r  "
            + "\n \n \n \n \n \n \n \n \n \n \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \" \"quoted\""
            + "It is a long established fact that a reader will be distracted by the readable content of a page when looking at its layout. The point of using Lorem Ipsum is that it has a more-or-less normal distribution of letters, as opposed to using 'Content here, content here', making it look like readable English. Many desktop publishing packages and web page editors now use Lorem Ipsum as their default model text, and a search for 'lorem ipsum' will uncover many web sites still in their infancy. Various versions have evolved over the years, sometimes by accident, sometimes on purpose injected humour and the like."
            + "SWAN also provides helpful extension methods for string manipulation").Split(new string[] { " " }, StringSplitOptions.None);

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
