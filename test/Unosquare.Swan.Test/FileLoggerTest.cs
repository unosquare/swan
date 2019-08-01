namespace Swan.Test
{
    using Components;
    using NUnit.Framework;
    using System.Threading.Tasks;
    using System;
    using System.IO;

    [TestFixture]
    public class FileLoggerTest
    {
        [Test]
        public async Task WithDefaultValues_FileExist()
        {
            FileLogger.Register();
            $"Test".Info();
            await Task.Delay(1);
            FileLogger.Unregister();

            var logPath = SwanRuntime.EntryAssemblyDirectory;

            var logFile = Path.Combine(logPath, $"Application_{DateTime.UtcNow:yyyyMMdd}.log");
            Assert.IsTrue(File.Exists(logFile));
        }

        [Test]
        public async Task WithDefaultValues_FileIsNotEmpty()
        {
            FileLogger.Register();
            $"Test".Info();
            await Task.Delay(1);
            FileLogger.Unregister();

            var logPath = SwanRuntime.EntryAssemblyDirectory;

            var logFile = Path.Combine(logPath, $"Application_{DateTime.UtcNow:yyyyMMdd}.log");
            var logContent = File.ReadAllText(logFile);
            Assert.IsNotEmpty(logContent);
        }

        [Test]
        public async Task WithCustomValues_FileExist()
        {
            var tempFile = Path.GetTempPath();
            FileLogger.Register(tempFile, false);
            $"Test".Info();
            await Task.Delay(1);
            FileLogger.Unregister();

            var logFile = Path.Combine(tempFile, $"Application.log");
            Assert.IsTrue(File.Exists(logFile));
        }

        [Test]
        public async Task WithCustomValues_FileIsNotEmpty()
        {
            var tempFile = Path.GetTempPath();
            FileLogger.Register(tempFile, false);
            $"Test".Info();
            await Task.Delay(1);
            FileLogger.Unregister();

            var logFile = Path.Combine(tempFile, $"Application.log");
            var logContent = File.ReadAllText(logFile);
            Assert.IsNotEmpty(logContent);
        }
    }
}
