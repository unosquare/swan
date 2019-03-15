namespace Unosquare.Swan.Test
{
    using Components;
    using NUnit.Framework;
    using System;
    using System.IO;

    [TestFixture]
    public class FileLoggerTest
    {
        [Test]
        public void WithDefaultValues_FileExist()
        {
            FileLogger.Register();
            $"Test".Info();
            FileLogger.Unregister();

            var logFile = Path.Combine(Runtime.EntryAssemblyDirectory, $"Application_{DateTime.UtcNow:yyyyMMdd}.log");
            Assert.IsTrue(File.Exists(logFile));
        }

        [Test]
        public void WithDefaultValues_FileIsNotEmpty()
        {
            FileLogger.Register();
            $"Test".Info();
            FileLogger.Unregister();

            var logFile = Path.Combine(Runtime.EntryAssemblyDirectory, $"Application_{DateTime.UtcNow:yyyyMMdd}.log");
            var logContent = File.ReadAllText(logFile);
            Assert.IsNotEmpty(logContent);
        }

        [Test]
        public void WithCustomValues_FileExist()
        {
            var tempFile = Path.GetTempPath();
            FileLogger.Register(tempFile, false);
            $"Test".Info();
            FileLogger.Unregister();

            var logFile = Path.Combine(tempFile, $"Application.log");
            Assert.IsTrue(File.Exists(logFile));
        }

        [Test]
        public void WithCustomValues_FileIsNotEmpty()
        {
            var tempFile = Path.GetTempPath();
            FileLogger.Register(tempFile, false);
            $"Test".Info();
            FileLogger.Unregister();

            var logFile = Path.Combine(tempFile, $"Application.log");
            var logContent = File.ReadAllText(logFile);
            Assert.IsNotEmpty(logContent);
        }
    }
}
