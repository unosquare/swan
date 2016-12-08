namespace Unosquare.Swan
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.ServiceProcess;
    using System.Threading;

    public static class WinServiceExtensions
    {
        private const string OnStartMethodName = "OnStart";
        private const string OnStopMethodName = "OnStop";

        public static void RunServicesInConsoleMode(this ServiceBase[] servicesToRun)
        {
            var onStartMethod = typeof(ServiceBase).GetMethod(OnStartMethodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            var onStopMethod = typeof(ServiceBase).GetMethod(OnStopMethodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            var serviceThreads = new List<Thread>();

            // TODO: Log
            //LogProvider.Current.Log("Starting services . . .");

            foreach (var service in servicesToRun)
            {
                var thread = new Thread(() =>
                {
                    onStartMethod.Invoke(service, new object[] { new string[] { } });
                    //LogProvider.Current.Log($"Started service '{service.GetType().Name}'");
                });

                serviceThreads.Add(thread);
                thread.Start();
            }

            //LogProvider.Current.Log("Press any key to stop all services.");
            Console.ReadLine();
            //LogProvider.Current.Log("Stopping services . . .");

            foreach (var service in servicesToRun)
            {
                onStopMethod.Invoke(service, null);
                //LogProvider.Current.Log($"Stopped service '{service.GetType().Name}'");
            }

            foreach (var thread in serviceThreads)
                thread.Join();

            //LogProvider.Current.Log("Stopped all services.");
        }
    }
}