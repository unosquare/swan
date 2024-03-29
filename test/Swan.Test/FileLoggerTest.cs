﻿namespace Swan.Test;

using Logging;

[TestFixture]
public class FileLoggerTest
{
    [Test]
    public async Task WithDefaultValues_FileExist()
    {
        if (!OperatingSystem.IsWindows())
            Assert.Ignore("Ignored");

        var instance = new FileLogger();
        Logger.RegisterLogger(instance);
        "Test".Info();
        await Task.Delay(1);
        Logger.UnregisterLogger(instance);

        Assert.IsTrue(File.Exists(instance.FilePath));
    }

    [Test]
    public async Task WithDefaultValues_FileIsNotEmpty()
    {
        if (!OperatingSystem.IsWindows())
            Assert.Ignore("Ignored");

        var instance = new FileLogger();
        Logger.RegisterLogger(instance);
        "Test".Info();
        await Task.Delay(1);
        Logger.UnregisterLogger(instance);

        var logContent = await File.ReadAllTextAsync(instance.FilePath);
        Assert.IsNotEmpty(logContent);
    }
}
