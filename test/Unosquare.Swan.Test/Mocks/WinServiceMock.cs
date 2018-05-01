#if !NETSTANDARD1_3 && !UWP
namespace Unosquare.Swan.Test.Mocks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
#if NET46
    using System.ServiceProcess;
#else
    using Abstractions;
#endif
    
    public class WinServiceMock : ServiceBase
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public WinServiceMock()
        {
            ServiceName = nameof(WinServiceMock);
            CanPauseAndContinue = false;
            AutoLog = true;
        }
        
        public int Counter { get; internal set; }

        internal async Task ProcessTask()
        {
            while (_cts.IsCancellationRequested == false)
            {
                Counter++;
                await Task.Delay(100);
            }
        }

        protected override void OnStart(string[] args)
        {
            Task.Factory.StartNew(ProcessTask, _cts.Token);
        }

        protected override void OnStop() => _cts.Cancel();
    }
}
#endif