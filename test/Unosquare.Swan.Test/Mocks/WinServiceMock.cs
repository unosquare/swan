#if NET452
using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace Unosquare.Swan.Test.Mocks
{
    public class WinServiceMock : ServiceBase
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        
        public WinServiceMock()
        {
            ServiceName = nameof(WinServiceMock);
            CanPauseAndContinue = false;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            Task.Factory.StartNew(ProcessTask, _cts.Token);
        }

        protected override void OnStop()
        {
            _cts.Cancel();
        }

        internal async Task ProcessTask()
        {
            while (_cts.IsCancellationRequested == false)
            {
                Counter++;
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        public int Counter { get; internal set; }
    }
}
#endif