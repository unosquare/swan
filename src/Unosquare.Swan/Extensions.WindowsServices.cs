namespace Unosquare.Swan
{
#if NET452
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.ServiceProcess;
#endif

    partial class Extensions
    {

#if NET452
        /// <summary>
        /// Runs a service in console mode.
        /// </summary>
        /// <param name="serviceToRun">The service to run.</param>
        public static void RunInConsoleMode(this ServiceBase serviceToRun)
        {
            RunInConsoleMode(new ServiceBase[] { serviceToRun });
        }

        /// <summary>
        /// Runs a set of services in console mode.
        /// </summary>
        /// <param name="servicesToRun">The services to run.</param>
        public static void RunInConsoleMode(this ServiceBase[] servicesToRun)
        {
            const string OnStartMethodName = "OnStart";
            const string OnStopMethodName = "OnStop";

            var onStartMethod = typeof(ServiceBase).GetMethod(OnStartMethodName,
                    BindingFlags.Instance | BindingFlags.NonPublic);
            var onStopMethod = typeof(ServiceBase).GetMethod(OnStopMethodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            var serviceThreads = new List<Thread>();

            "Starting services . . .".Info();

            foreach (var service in servicesToRun)
            {
                var thread = new Thread(() =>
                {
                    onStartMethod.Invoke(service, new object[] { new string[] { } });
                    $"Started service '{service.GetType().Name}'".Info();
                });

                serviceThreads.Add(thread);
                thread.Start();
            }

            "Press any key to stop all services.".ReadKey();
            "Stopping services . . .".Info();

            foreach (var service in servicesToRun)
            {
                onStopMethod.Invoke(service, null);
                $"Stopped service '{service.GetType().Name}'".Info();
            }

            foreach (var thread in serviceThreads)
                thread.Join();

            "Stopped all services.".Info();
        }
#endif

    }
}
