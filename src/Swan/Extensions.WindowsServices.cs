namespace Swan
{
    using Logging;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
#if NET461
    using System.ServiceProcess;
#else
    using Services;
#endif

    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class WindowsServicesExtensions
    {
        /// <summary>
        /// Runs a service in console mode.
        /// </summary>
        /// <param name="this">The service to run.</param>
        /// <param name="loggerSource">The logger source.</param>
        /// <exception cref="ArgumentNullException">this.</exception>
        public static void RunInConsoleMode(this ServiceBase @this, string loggerSource = null)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            RunInConsoleMode(new[] { @this }, loggerSource);
        }

        /// <summary>
        /// Runs a set of services in console mode.
        /// </summary>
        /// <param name="this">The services to run.</param>
        /// <param name="loggerSource">The logger source.</param>
        /// <exception cref="ArgumentNullException">this.</exception>
        /// <exception cref="InvalidOperationException">The ServiceBase class isn't available.</exception>
        public static void RunInConsoleMode(this ServiceBase[] @this, string loggerSource = null)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            const string onStartMethodName = "OnStart";
            const string onStopMethodName = "OnStop";

            var onStartMethod = typeof(ServiceBase).GetMethod(onStartMethodName,
                    BindingFlags.Instance | BindingFlags.NonPublic);
            var onStopMethod = typeof(ServiceBase).GetMethod(onStopMethodName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (onStartMethod == null || onStopMethod == null)
                throw new InvalidOperationException("The ServiceBase class isn't available.");

            var serviceThreads = new List<Thread>();
            "Starting services . . .".Info(loggerSource ?? SwanRuntime.EntryAssemblyName.Name);

            foreach (var service in @this)
            {
                var thread = new Thread(() =>
                {
                    onStartMethod.Invoke(service, new object[] { Array.Empty<string>() });
                    $"Started service '{service.GetType().Name}'".Info(loggerSource ?? service.GetType().Name);
                });

                serviceThreads.Add(thread);
                thread.Start();
            }

            "Press any key to stop all services.".Info(loggerSource ?? SwanRuntime.EntryAssemblyName.Name);
            Terminal.ReadKey(true, true);
            "Stopping services . . .".Info(SwanRuntime.EntryAssemblyName.Name);

            foreach (var service in @this)
            {
                onStopMethod.Invoke(service, null);
                $"Stopped service '{service.GetType().Name}'".Info(loggerSource ?? service.GetType().Name);
            }

            foreach (var thread in serviceThreads)
                thread.Join();

            "Stopped all services.".Info(loggerSource ?? SwanRuntime.EntryAssemblyName.Name);
        }
    }
}