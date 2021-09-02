using Swan.DependencyInjection;
using Swan.Extensions;
using Swan.Formatters;
using Swan.Logging;
using Swan.Mapping;
using Swan.Net;
using Swan.Net.Dns;
using Swan.Platform;
using Swan.Reflection;
using Swan.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Swan.Samples
{
    class SubDict : Dictionary<string, object>
    {

    }

    class MockInfo
    {
        public string Name { get; set; } = "Frrrrr";

        public int Value { get; set; } = 2332455;

        public TimeSpan Ts { get; set; }

        public DateTime Dt { get; set; }

        public Guid Guid { get; set; }
    }

    public static partial class Program
    {

        private static void Sketchpad()
        {
            var newList = typeof(List<int>).TypeInfo().CreateInstance();

            bool boolValue = true;
            var atomic = new AtomicBoolean(false);
            boolValue = atomic;

            var simpleInt = 0;
            var atomic2 = new AtomicInteger(-1024);
            simpleInt = atomic2;
            var isZero = atomic2 >= simpleInt;
            atomic2.Increment();

            var d = new Dictionary<int, string>
            {
                [1] = "Hello",
                [2] = "World"
            };

            if (d is IDictionary dictionary)
            {

            }

            dynamic expando = new ExpandoObject();
            expando.Property1 = "one property";
            expando.Property2 = new[] { 6, 7, 8, 9, 10 };

            var mock = new MockInfo();
            var sdict = new SubDict
            {
                ["Hello"] = 43,
                ["World"] = "Some text here",
                ["array"] = new[] { 6, 7, 8, 9, 10 },
                ["object"] = new Dictionary<string, string>
                {
                    ["x"] = "Y"
                },
                ["dynamic"] = expando
            };

            var outText0 = ProxySerializer.Serialize(sdict);

            var outDict = System.Text.Json.JsonSerializer.Deserialize(outText0, typeof(SubDict));

            var outText = ProxySerializer.Serialize(d);
            var outText2 = ProxySerializer.Serialize(mock);

            var objDict = new Dictionary<object, object?>()
            {
                [TimeSpan.FromMilliseconds(334344)] = sdict,
                [mock] = sdict,
                ["object"] = new MockInfo { Name = "OTHER" }
            };

            var objDictJson = ProxySerializer.Serialize(objDict);
        }

        /// <summary>
        /// Entry point of the Program.
        /// </summary>
        public static async Task Main()
        {

            Sketchpad();

            LoggerExtensions.RegisterLogger<FileLogger>();

            TestJson();
            TestApplicationInfo();
            await TestTerminalOutputs();
            try
            {
                await TestNetworkUtilities();
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                Terminal.WriteLine($"Error testing network {ex}", ConsoleColor.Red, TerminalWriterFlags.StandardError);
            }
            TestContainerAndMessageHub();
            TestExceptionLogging();

            TestFastOutput();
            TestReadPrompt();
            TestCsvFormatters();
            Terminal.Flush();
            Terminal.ReadKey("Enter any key to exit . . .");
        }

        private static void TestExceptionLogging()
        {
            try
            {
                throw new SampleException();
            }
            catch (Exception ex)
            {
                ex.Log(typeof(Program), "Exception dump starts");
            }
        }

        private static void TestApplicationInfo()
        {
            Terminal.WriteWelcomeBanner();
            $"Operating System Type: {Environment.OSVersion}    CLR Type: {(SwanRuntime.IsUsingMonoRuntime ? "Mono" : ".NET")}".Info();
            $"Local Storage Path: {SwanRuntime.LocalStoragePath}".Info();
        }

        private static void TestJson()
        {
            var instance = new SampleCopyTarget
            { AlternateId = 10, CreationDate = new DateTime(2010, 1, 1), Id = 1, Score = "A" };

            var payload = instance.JsonSerialize();

            payload.Info(typeof(Program));

            var recover = payload.JsonDeserialize<SampleCopyTarget>();

            recover.Dump(typeof(Program));

            var jsonText =
                "{\"SimpleProperty\": \"SimpleValue\", \"EmptyProperty\": \"\\/Forward-Slash\\\"\", \"EmptyArray\": [], \"EmptyObject\": {}}";
            var jsonObject = JsonSerializer.Deserialize<object>(jsonText);
            jsonObject.Dump(typeof(Program));

            jsonText =
                "{\"SimpleProperty\": \r\n     \"SimpleValue\", \"EmptyProperty\": \" \", \"EmptyArray\": [  \r\n \r\n  ], \"EmptyObject\": { } \r\n, \"NumberStringArray\": [1,2,\"hello\",4,\"666\",{ \"NestedObject\":true }] }";
            jsonObject = jsonText.JsonDeserialize();
            jsonObject.Dump(typeof(Program));

            "test".Dump(typeof(Program));
        }

        private static async Task TestNetworkUtilities()
        {
            const string domainName = "unosquare.com";
            const string ntpServer = "time.windows.com";

            var dnsServers = Network.GetIPv4DnsServers();
            var privateIPs = Network.GetIPv4Addresses(false);
            var publicIP = await Network.GetPublicIPAddressAsync();
            var dnsLookup = await Network.GetDnsHostEntryAsync(domainName);
            var ptrRecord = await Network.GetDnsPointerEntryAsync(publicIP);
            var mxRecords = await Network.QueryDnsAsync("unosquare.com", DnsRecordType.MX);
            var txtRecords = await Network.QueryDnsAsync("unosquare.com", DnsRecordType.TXT);
            var ntpTime = await Network.GetNetworkTimeUtcAsync(ntpServer);

            $"NTP Time   : [{ntpServer}]: [{ntpTime.ToSortableDateTime()}]".Info(nameof(Network));
            $"Private IPs: [{string.Join(", ", privateIPs.Select(p => p.ToString()))}]".Info(nameof(Network));
            $"DNS Servers: [{string.Join(", ", dnsServers.Select(p => p.ToString()))}]".Info(nameof(Network));
            $"Public IP  : [{publicIP}]".Info(nameof(Network));
            $"Reverse DNS: [{publicIP}]: [{ptrRecord}]".Info(nameof(Network));
            $"Lookup DNS : [{domainName}]: [{string.Join("; ", dnsLookup.Select(p => p.ToString()))}]".Info(
                nameof(Network));
            $"Query MX   : [{domainName}]: [{mxRecords.AnswerRecords.First().MailExchangerPreference} {mxRecords.AnswerRecords.First().MailExchangerDomainName}]"
                .Info(nameof(Network));
            $"Query TXT  : [{domainName}]: [{string.Join("; ", txtRecords.AnswerRecords.Select(t => t.DataText))}]"
                .Info(nameof(Network));
        }

        private static void TestContainerAndMessageHub()
        {
            DependencyContainer.Current.Register<ISampleAnimal, SampleFish>();
            $"The concrete type ended up being: {DependencyContainer.Current.Resolve<ISampleAnimal>().Name}".Warn();
            DependencyContainer.Current.Unregister<ISampleAnimal>();
            DependencyContainer.Current.Register<ISampleAnimal, SampleMonkey>();
            $"The concrete type ended up being: {DependencyContainer.Current.Resolve<ISampleAnimal>().Name}".Warn();
        }

        private static void TestFastOutput()
        {
            var limit = Console.BufferHeight;
            for (var i = 0; i < limit; i += 25)
            {
                Terminal.WriteLine($"Output info {i} ({((decimal)i / limit):P})");
                Terminal.BacklineCursor();
            }
        }

        private static void TestReadPrompt()
        {
            Terminal.Clear();
            var sampleOptions = new Dictionary<ConsoleKey, string>
            {
                {ConsoleKey.A, "Sample A"},
                {ConsoleKey.B, "Sample B"},
                {ConsoleKey.C, "Sample C" },
                {ConsoleKey.D, "Sample D" },
                {ConsoleKey.E, "Sample E" }
            };

            Terminal.ReadPrompt("Please provide an option", sampleOptions, "Exit this program");
        }

        private static async Task TestTerminalOutputs()
        {
            for (var i = 0; i <= 100; i++)
            {
                await Task.Delay(20);
                Terminal.OverwriteLine($"Current Progress: {(i + "%"),-10}");
            }

            if (Terminal.ReadKey("Press a key to test logging output. (X) will exit.").Key == ConsoleKey.X) return;
            Terminal.WriteLine("OUTPUT LOGGING TEST", ConsoleColor.Blue);
            "This is some error".Error(typeof(Program));
            "This is some error".Error(nameof(TestTerminalOutputs));
            "This is some info".Info(typeof(Program));
            "This is some info".Info(nameof(TestTerminalOutputs));
            "This is some warning".Warn(typeof(Program));
            "This is some warning".Warn(nameof(TestTerminalOutputs));
            "This is some tracing info".Trace(typeof(Program));
            "This is some tracing info".Trace(nameof(TestTerminalOutputs));
            "This is for debugging stuff".Debug(typeof(Program));
            "This is for debugging stuff".Debug(nameof(TestTerminalOutputs));

            // The simplest way of writing a line of text:
            Terminal.WriteLine($"Hello, today is {DateTime.Today}");

            // Now, add some color:
            Terminal.WriteLine($"Hello, today is {DateTime.Today}", ConsoleColor.Green);

            if (Terminal.ReadKey("Press a key to test menu options. (X) will exit.").Key == ConsoleKey.X) return;
            Terminal.WriteLine("TESTING MENU OPTIONS", ConsoleColor.Blue);

            var sampleOptions = new Dictionary<ConsoleKey, string>
            {
                {ConsoleKey.A, "Sample A"},
                {ConsoleKey.B, "Sample B"}
            };

            Terminal.ReadPrompt("Please provide an option", sampleOptions, "Exit this program");
        }

        private static void TestCsvFormatters()
        {
            var action = new Action(() =>
            {
                var test01FilePath = SwanRuntime.GetDesktopFilePath("csv-writer-test-01.csv");
                var test02FilePath = SwanRuntime.GetDesktopFilePath("csv-writer-test-02.csv");

                var generatedRecords = SampleCsvRecord.CreateSampleSet(100);
                $"Generated {generatedRecords.Count} sample records.".Info(nameof(TestCsvFormatters));

                var savedRecordCount = CsvWriter.SaveRecords(generatedRecords, test01FilePath);
                $"Saved {savedRecordCount} records (including header) to file: {Path.GetFileName(test01FilePath)}."
                    .Info(nameof(TestCsvFormatters));

                var loadedRecords = CsvReader.LoadRecords<SampleCsvRecord>(test01FilePath);
                $"Loaded {loadedRecords.Count} records from file: {Path.GetFileName(test01FilePath)}.".Info(
                    nameof(TestCsvFormatters));

                savedRecordCount = CsvWriter.SaveRecords(generatedRecords, test02FilePath);
                $"Saved {savedRecordCount} records (including header) to file: {Path.GetFileName(test02FilePath)}."
                    .Info(nameof(TestCsvFormatters));

                var sourceObject = loadedRecords[generatedRecords.Count / 2];
                var targetObject = new SampleCopyTarget();
                var copiedProperties = sourceObject.CopyPropertiesTo(targetObject);
                $"{nameof(MappingExtensions.CopyPropertiesTo)} method copied {copiedProperties} properties from one object to another"
                    .Info(nameof(TestCsvFormatters));
            });
        }
    }
}