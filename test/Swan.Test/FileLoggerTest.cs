namespace Swan.Test
{
    using Logging;
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
            var instance = new FileLogger();
            Logger.RegisterLogger(instance);
            "Test".Info();
            await Task.Delay(1);
            // TODO: Unregister
            
            Assert.IsTrue(File.Exists(instance.LogPath));
        }

        [Test]
        public async Task WithDefaultValues_FileIsNotEmpty()
        {
            Logger.RegisterLogger(new FileLogger());
            "Test".Info();
            await Task.Delay(1);
            // TODO: Unregister
            var logPath = SwanRuntime.EntryAssemblyDirectory;

            var logFile = Path.Combine(logPath, $"Application_{DateTime.UtcNow:yyyyMMdd}.log");
            var logContent = File.ReadAllText(logFile);
            Assert.IsNotEmpty(logContent);
        }

        [Test]
        public async Task WithCustomValues_FileExist()
        {
            var tempFile = Path.GetTempPath();
            Logger.RegisterLogger(new FileLogger(tempFile, false));
            "Test".Info();
            await Task.Delay(1);

            // TODO: Unregister
            var logFile = Path.Combine(tempFile, $"Application.log");
            Assert.IsTrue(File.Exists(logFile));
        }

        [Test]
        public async Task WithCustomValues_FileIsNotEmpty()
        {
            var tempFile = Path.GetTempPath();
            Logger.RegisterLogger(new FileLogger(tempFile, false));
            "Test".Info();
            await Task.Delay(1);

            // TODO: Unregister
            var logFile = Path.Combine(tempFile, "Application.log");
            var logContent = File.ReadAllText(logFile);
            Assert.IsNotEmpty(logContent);
        }
    }
}
