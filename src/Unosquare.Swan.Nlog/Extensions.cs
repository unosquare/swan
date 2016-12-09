using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Diagnostics;

namespace Unosquare.Swan.Nlog
{
    public static class Extensions
    {
        /// <summary>
        /// Configures the logging.
        /// </summary>
        /// <param name="targets">The targets.</param>
        public static void ConfigureLogging(params Target[] targets)
        {
            ConfigureLogging(Debugger.IsAttached, targets);
        }

        /// <summary>
        /// Configures the logging.
        /// </summary>
        /// <param name="isDebug">if set to <c>true</c> [is debug].</param>
        /// <param name="targets">The targets.</param>
        public static void ConfigureLogging(bool isDebug, params Target[] targets)
        {
            var config = new LoggingConfiguration();

            // TODO: pending AppDbLog
            //var liteTarget = new AppDbLogTarget();
            //config.AddTarget(nameof(AppDbLogTarget), liteTarget);
            //config.AddRule(isDebug ? LogLevel.Trace : LogLevel.Info, LogLevel.Fatal, liteTarget);

            foreach (var target in targets)
            {
                config.AddTarget(target.GetType().Name, target);
                config.AddRule(isDebug ? LogLevel.Trace : LogLevel.Info, LogLevel.Fatal, target);
            }

            if (isDebug)
            {
                var consoleTarget = new ColoredConsoleTarget
                {
                    Layout = @"${pad:padding=14:fixedLength=true:inner=${date:format=HH\:mm\:ss.fff}} " +
                             "${pad:padding=6:fixedLength=true:inner=${level:uppercase=true}} " +
                             "${pad:padding=18:fixedLength=true:inner=${logger:shortName=true}} " +
                             "SessionId ${event-properties:item=SessionId} - ${message}"
                };

                var asyncConsoleTarget = new AsyncTargetWrapper(consoleTarget, 500,
                    AsyncTargetWrapperOverflowAction.Discard);

                config.AddTarget(nameof(Console), asyncConsoleTarget);
                config.AddRule(LogLevel.Trace, LogLevel.Fatal, asyncConsoleTarget);
            }

            LogManager.Configuration = config;
        }
    }
}