#if !NETSTANDARD1_3 && !UWP
namespace Unosquare.Swan
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
#if NET452
    using System.Reflection;
    using System.ServiceProcess;
#else
    using Abstractions;
#endif

    /// <summary>
    /// Extension methods
    /// </summary>
    public static class WindowsServicesExtensions
    {
        /// <summary>
        /// Runs a service in console mode.
        /// </summary>
        /// <param name="serviceToRun">The service to run.</param>
#if NET452
        public static void RunInConsoleMode(this ServiceBase serviceToRun)
        {
            if (serviceToRun == null)
                throw new ArgumentNullException(nameof(serviceToRun));

            RunInConsoleMode(new ServiceBase[] { serviceToRun });
        }
#else
        public static void RunInConsoleMode(this IServiceBase serviceToRun)
        {
            if (serviceToRun == null)
                throw new ArgumentNullException(nameof(serviceToRun));

            RunInConsoleMode(new[] {serviceToRun});
        }
#endif

        /// <summary>
        /// Runs a set of services in console mode.
        /// </summary>
        /// <param name="servicesToRun">The services to run.</param>
#if NET452
        public static void RunInConsoleMode(this ServiceBase[] servicesToRun)
#else
        public static void RunInConsoleMode(this IServiceBase[] servicesToRun)
#endif
        {
            if (servicesToRun == null)
                throw new ArgumentNullException(nameof(servicesToRun));

#if NET452
            const string onStartMethodName = "OnStart";
            const string onStopMethodName = "OnStop";

            var onStartMethod = typeof(ServiceBase).GetMethod(onStartMethodName,
                    BindingFlags.Instance | BindingFlags.NonPublic);
            var onStopMethod = typeof(ServiceBase).GetMethod(onStopMethodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
#endif

            var serviceThreads = new List<Thread>();
            "Starting services . . .".Info(Runtime.EntryAssemblyName.Name);

            foreach (var service in servicesToRun)
            {
                var thread = new Thread(() =>
                {
#if NET452
                    onStartMethod.Invoke(service, new object[] { new string[] { } });
#else
                    service.OnStart(new string[] { });
#endif
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
#if NET452
                onStopMethod.Invoke(service, null);
#else
                service.OnStop();
#endif
                $"Stopped service '{service.GetType().Name}'".Info(service.GetType());
            }

            foreach (var thread in serviceThreads)
                thread.Join();

            "Stopped all services.".Info(Runtime.EntryAssemblyName.Name);
        }
    }
}
#endif