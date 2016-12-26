namespace Unosquare.Swan.Samples
{
    using Abstractions;
    using Formatters;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Utilities;

    public partial class Program
    {
        /// <summary>
        /// Mains the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="SampleException"></exception>
        public static void Main(string[] args)
        {
            TestApplicationInfo();
            //TestNetworkUtilities();
            TestContainerAndMessageHub();
            TestJson();
            TestExceptionLogging();
            //TestTerminalOutputs();
            //TestCsvFormatters();


            

            "Enter any key to exit . . .".ReadKey();
        }

        static void TestExceptionLogging()
        {
            try
            {
                throw new SampleException();
            }
            catch (Exception ex)
            {
                ex.Log(null, "Exception dump starts");
            }
        }

        static void TestApplicationInfo()
        {
            CurrentApp.WriteWelcomeBanner();
            $"Operating System Type: {CurrentApp.OS}    CLR Type: {(CurrentApp.IsUsingMonoRuntime ? "Mono" : ".NET")}".Info();
            $"Local Storage Path: {CurrentApp.LocalStoragePath}".Info();
            $"Process Id: {CurrentApp.Process.Id}".Info();
        }

        static void TestJson()
        {
            var jsonText = "{\"SimpleProperty\": \"SimpleValue\", \"EmptyProperty\": \"\\/Forward-Slash\\\"\", \"EmptyArray\": [], \"EmptyObject\": {}}";
            var jsonObject = Json.Deserialize(jsonText);
            jsonObject.Dump();

            jsonText = "{\"SimpleProperty\": \"SimpleValue\", \"EmptyProperty\": \" \", \"EmptyArray\": [    ], \"EmptyObject\": {  }, \"NumberStringArray\": [1,2,\"hello\",4,\"666\",{ \"NestedObject\":true }] }";
            jsonObject = Json.Deserialize(jsonText);
            jsonObject.Dump();

            //var jsonTextData = "{\"Text\":\"Hello. We will try some special chars: New Line: \\r \\n Quotes: \\\" / Special Chars: \\u0323 \\u0003 \\u1245\", \"EmptyObject\": {}, \"EmptyArray\": [], \"SomeDate\": \"/" + DateTime.Now.ToStringInvariant() + "/\" }";
            //var jsonParsedData = Json.Deserialize(jsonTextData);

        }

        static void TestNetworkUtilities()
        {
            var domainName = "unosquare.com";
            var ntpServer = "time.windows.com";

            var dnsServers = Network.GetIPv4DnsServers();
            var privateIPs = Network.GetIPv4Addresses(false);
            var publicIP = Network.GetPublicIPAddress();
            var dnsLookup = Network.GetDnsHostEntry(domainName);
            var ptrRecord = Network.GetDnsPointerEntry(publicIP);
            var mxRecords = Network.QueryDns("unosquare.com", DnsRecordType.MX);
            var txtRecords = Network.QueryDns("unosquare.com", DnsRecordType.TXT);
            var ntpTime = Network.GetNetworkTimeUtc(ntpServer);

            $"NTP Time   : [{ntpServer}]: [{ntpTime.ToSortableDateTime()}]".Info(nameof(Network));
            $"Private IPs: [{string.Join(", ", privateIPs.Select(p => p.ToString()))}]".Info(nameof(Network));
            $"DNS Servers: [{string.Join(", ", dnsServers.Select(p => p.ToString()))}]".Info(nameof(Network));
            $"Public IP  : [{publicIP.ToString()}]".Info(nameof(Network));
            $"Reverse DNS: [{publicIP.ToString()}]: [{ptrRecord}]".Info(nameof(Network));
            $"Lookup DNS : [{domainName}]: [{string.Join("; ", dnsLookup.Select(p => p.ToString()))}]".Info(nameof(Network));
            $"Query MX   : [{domainName}]: [{mxRecords.AnswerRecords.First().MailExchangerPreference} {mxRecords.AnswerRecords.First().MailExchangerDomainName}]".Info(nameof(Network));
            $"Query TXT  : [{domainName}]: [{string.Join("; ", txtRecords.AnswerRecords.Select(t => t.DataText))}]".Info(nameof(Network));
        }

        static void TestSingleton()
        {
            SampleSingleton.Instance.Name.Info(nameof(SampleSingleton));
        }

        static void TestContainerAndMessageHub()
        {
            CurrentApp.Container.Register<ISampleAnimal, SampleFish>();
            $"The concrete type ended up being: {CurrentApp.Container.Resolve<ISampleAnimal>().Name}".Warn();
            CurrentApp.Container.Unregister<ISampleAnimal>();
            CurrentApp.Container.Register<ISampleAnimal, SampleMonkey>();
            $"The concrete type ended up being: {CurrentApp.Container.Resolve<ISampleAnimal>().Name}".Warn();

            CurrentApp.Messages.Subscribe<SampleMessage>((m) => { $"Received the following message from '{m.Sender}': '{m.Content}'".Trace(); });
            CurrentApp.Messages.Publish(new SampleMessage("SENDER HERE", "This is some sample text"));
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
                var targetObject = new SampleCopyTarget();
                var copiedProperties = sourceObject.CopyPropertiesTo(targetObject);
                $"{nameof(Extensions.CopyPropertiesTo)} method copied {copiedProperties} properties from one object to another".Info(nameof(TestCsvFormatters));
            });

            var elapsed = action.Benchmark();
            $"Elapsed: {Math.Round(elapsed.TotalMilliseconds, 3)} milliseconds".Trace();
        }
    }

    
}
