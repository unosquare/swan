using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.Swan.Abstractions;

namespace Unosquare.Swan.Test.Mocks
{
    public class AppWorkerMock : AppWorkerBase
    {
        public Exception Exception { get; private set; }
        public bool ExitBecauseCancellation { get; private set; }
        public int Count { get; private set; }

        public Action OnExit { get; set; }

        protected override void WorkerThreadLoop()
        {
            while (CancellationPending == false)
            {
                Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
                Count++;

                if (Count == 6) throw new Exception();
            }

            ExitBecauseCancellation = true;
        }

        protected override void OnWorkerThreadExit()
        {
            base.OnWorkerThreadExit();
            OnExit?.Invoke();
        }

        protected override void OnWorkerThreadLoopException(Exception ex)
        {
            Exception = ex;
            base.OnWorkerThreadLoopException(ex);
        }
    }
}