﻿namespace Unosquare.Swan.Samples
{
    using System.Threading.Tasks;
    using Components;
    using Networking.Ldap;
    using Formatters;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Networking;
    using System.Collections;

    public partial class Program
    {
        /// <summary>
        /// Mains the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="SampleException"></exception>
        public static async Task Main(string[] args)
        {
            Runtime.WriteWelcomeBanner(ConsoleColor.Green);
            TestApplicationInfo();
            await TestTerminalOutputs();

#if !NET46
            await TestLdapModify();
            await TestLdapSearch();
#endif
            TestNetworkUtilities();
            //TestContainerAndMessageHub();
            //TestJson();
            //TestExceptionLogging();

            //TestFastOutputAndReadPrompt();
            // TestCsvFormatters();
            //Terminal.Flush();
            "Enter any key to exit . . .".ReadKey();
        }

        static async Task TestLdapSearch()
        {
            try
            {
                var cn = new LdapConnection();

                await cn.Connect("ldap.forumsys.com", 389);
                await cn.Bind("uid=riemann,dc=example,dc=com", "password");
                var lsc = await cn.Search("ou=scientists,dc=example,dc=com", LdapConnection.ScopeSub);

                while (lsc.HasMore())
                {
                    var entry = lsc.Next();
                    var ldapAttributes = entry.GetAttributeSet();


                    Console.WriteLine(
                        $"{ldapAttributes.GetAttribute("uniqueMember")?.StringValue ?? string.Empty}");
                }

                //While all the entries are parsed, disconnect
                cn.Disconnect();
            }
            catch (Exception ex)
            {
                ex.Error(nameof(Main), "Error LDAP");
            }
        }

        static async Task TestLdapModify()
        {
            try
            {
                var cn = new LdapConnection();

                await cn.Connect("ad.unosquare.com", 389);
                await cn.Bind("@unosquare.com", "password");
                await cn.Modify("cn=,ou=Employees,dc=ad,dc=unosquare,dc=com",
                    new[] {new LdapModification(LdapModificationOp.Replace, "mobile", "33366669999")});
                cn.Disconnect();
            }
            catch (Exception ex)
            {
                ex.Error(nameof(Main), "Error LDAP");
            }
        }

        static void TestExceptionLogging()
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

        static void TestApplicationInfo()
        {
            Runtime.WriteWelcomeBanner();
            $"Operating System Type: {Runtime.OS}    CLR Type: {(Runtime.IsUsingMonoRuntime ? "Mono" : ".NET")}".Info();
            $"Local Storage Path: {Runtime.LocalStoragePath}".Info();
        }

        static void TestJson()
        {
            var jsonText =
                "{\"SimpleProperty\": \"SimpleValue\", \"EmptyProperty\": \"\\/Forward-Slash\\\"\", \"EmptyArray\": [], \"EmptyObject\": {}}";
            var jsonObject = Json.Deserialize(jsonText);
            jsonObject.Dump(typeof(Program));

            jsonText =
                "{\"SimpleProperty\": \r\n     \"SimpleValue\", \"EmptyProperty\": \" \", \"EmptyArray\": [  \r\n \r\n  ], \"EmptyObject\": { } \r\n, \"NumberStringArray\": [1,2,\"hello\",4,\"666\",{ \"NestedObject\":true }] }";
            jsonObject = Json.Deserialize(jsonText);
            jsonObject.Dump(typeof(Program));

            "test".Dump(typeof(Program));
        }

        static void TestNetworkUtilities()
        {
            const string domainName = "unosquare.com";
            const string ntpServer = "time.windows.com";

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
            $"Public IP  : [{publicIP}]".Info(nameof(Network));
            $"Reverse DNS: [{publicIP}]: [{ptrRecord}]".Info(nameof(Network));
            $"Lookup DNS : [{domainName}]: [{string.Join("; ", dnsLookup.Select(p => p.ToString()))}]".Info(
                nameof(Network));
            $"Query MX   : [{domainName}]: [{mxRecords.AnswerRecords.First().MailExchangerPreference} {mxRecords.AnswerRecords.First().MailExchangerDomainName}]"
                .Info(nameof(Network));
            $"Query TXT  : [{domainName}]: [{string.Join("; ", txtRecords.AnswerRecords.Select(t => t.DataText))}]"
                .Info(nameof(Network));
        }

        static void TestContainerAndMessageHub()
        {
            DependencyContainer.Current.Register<ISampleAnimal, SampleFish>();
            $"The concrete type ended up being: {DependencyContainer.Current.Resolve<ISampleAnimal>().Name}".Warn();
            DependencyContainer.Current.Unregister<ISampleAnimal>();
            DependencyContainer.Current.Register<ISampleAnimal, SampleMonkey>();
            $"The concrete type ended up being: {DependencyContainer.Current.Resolve<ISampleAnimal>().Name}".Warn();

            var messageHub = DependencyContainer.Current.Resolve<IMessageHub>() as MessageHub;
            messageHub.Subscribe<SampleMessage>((m) =>
            {
                $"Received the following message from '{m.Sender}': '{m.Content}'".Trace();
            });
            messageHub.Publish(new SampleMessage("SENDER HERE", "This is some sample text"));
        }

        static void TestFastOutputAndReadPrompt()
        {
            var limit = Console.BufferHeight;
            for (var i = 0; i < limit; i += 25)
            {
                $"Output info {i} ({((decimal) i / limit):P})".Info(typeof(Program));
                Terminal.BacklineCursor();
            }

            var sampleOptions = new Dictionary<ConsoleKey, string>
            {
                {ConsoleKey.A, "Sample A"},
                {ConsoleKey.B, "Sample B"}
            };

            "Please provide an option".ReadPrompt(sampleOptions, "Exit this program");
        }

        static async Task TestTerminalOutputs()
        {
            for (var i = 0; i <= 100; i++)
            {
                await Task.Delay(20);
                $"Current Progress: {(i + "%"),-10}".OverwriteLine();
            }

            if ("Press a key to output the current codepage. (X) will exit.".ReadKey().Key == ConsoleKey.X) return;
            "CODEPAGE TEST".WriteLine(ConsoleColor.Blue);
            Terminal.PrintCurrentCodePage();

            if ("Press a key to test logging output. (X) will exit.".ReadKey().Key == ConsoleKey.X) return;
            "OUTPUT LOGGING TEST".WriteLine(ConsoleColor.Blue);
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
            $"Hello, today is {DateTime.Today}".WriteLine();

            // A slightly better way:
            $"Hello, today is {DateTime.Today}".WriteLine();

            // Now, add some color:
            $"Hello, today is {DateTime.Today}".WriteLine(ConsoleColor.Green);

            // Write it out to the debugger as well!
            $"Hello, today is {DateTime.Today}".WriteLine(ConsoleColor.Green,
                TerminalWriters.StandardOutput | TerminalWriters.Diagnostics);

            // You could have also skipped the color argument and just use the default
            $"Hello, today is {DateTime.Today}".WriteLine(null,
                TerminalWriters.StandardOutput | TerminalWriters.Diagnostics);

            if ("Press a key to test menu options. (X) will exit.".ReadKey().Key == ConsoleKey.X) return;
            "TESTING MENU OPTIONS".WriteLine(ConsoleColor.Blue);

            var sampleOptions = new Dictionary<ConsoleKey, string>
            {
                {ConsoleKey.A, "Sample A"},
                {ConsoleKey.B, "Sample B"}
            };

            "Please provide an option".ReadPrompt(sampleOptions, "Exit this program");
        }

        static void TestCsvFormatters()
        {
            var action = new Action(() =>
            {
                var test01FilePath = Runtime.GetDesktopFilePath("csv-writer-test-01.csv");
                var test02FilePath = Runtime.GetDesktopFilePath("csv-witer-test-02.csv");

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
                $"{nameof(Extensions.CopyPropertiesTo)} method copied {copiedProperties} properties from one object to another"
                    .Info(nameof(TestCsvFormatters));
            });

            var elapsed = action.Benchmark();
            $"Elapsed: {Math.Round(elapsed.TotalMilliseconds, 3)} milliseconds".Trace();
        }
    }
}