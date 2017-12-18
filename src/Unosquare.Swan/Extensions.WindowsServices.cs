#if NET452
namespace Unosquare.Swan
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.ServiceProcess;

    /// <summary>
    /// Extension methods
    /// </summary>
    public static class WindowsServicesExtensions
    {
        /// <summary>
        /// Runs a service in console mode.
        /// </summary>
        /// <param name="serviceToRun">The service to run.</param>
        public static void RunInConsoleMode(this ServiceBase serviceToRun)
        {
            if (serviceToRun == null)
                throw new ArgumentNullException(nameof(serviceToRun));

            RunInConsoleMode(new ServiceBase[] { serviceToRun });
        }

        /// <summary>
        /// Runs a set of services in console mode.
        /// </summary>
        /// <param name="servicesToRun">The services to run.</param>
        public static void RunInConsoleMode(this ServiceBase[] servicesToRun)
        {
            if (servicesToRun == null)
                throw new ArgumentNullException(nameof(servicesToRun));

            const string onStartMethodName = "OnStart";
            const string onStopMethodName = "OnStop";

            var onStartMethod = typeof(ServiceBase).GetMethod(onStartMethodName,
                    BindingFlags.Instance | BindingFlags.NonPublic);
            var onStopMethod = typeof(ServiceBase).GetMethod(onStopMethodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            var serviceThreads = new List<Thread>();

            "Starting services . . .".Info(Runtime.EntryAssemblyName.Name);

            foreach (var service in servicesToRun)
            {
                var thread = new Thread(() =>
                {
                    onStartMethod.Invoke(service, new object[] { new string[] { } });
                    $"Started service '{service.GetType().Name}'".Info(service.GetType());
                });

                serviceThreads.Add(thread);
                thread.Start();
            }

            "Press any key to stop all services.".Info(Runtime.EntryAssemblyName.Name);
            Terminal.ReadKey(true, true);
            "Stopping services . . .".Info(Runtime.EntryAssemblyName.Name);

            foreach (var service in servicesToRun)
            {
                onStopMethod.Invoke(service, null);
                $"Stopped service '{service.GetType().Name}'".Info(service.GetType());
            }

            foreach (var thread in serviceThreads)
                thread.Join();

            "Stopped all services.".Info(Runtime.EntryAssemblyName.Name);
        }
    }
}
#endif