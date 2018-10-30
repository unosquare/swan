namespace Unosquare.Swan.Test.Mocks
{
    using System;
    using System.Threading.Tasks;
    using Abstractions;

    public class AppWorkerMock : AppWorkerBase
    {
        public Exception Exception { get; private set; }
        public bool ExitBecauseCancellation { get; private set; } = true;
        public int Count { get; private set; }

        public Action OnExit { get; set; }

        protected override async Task WorkerThreadLoop()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100), CancellationToken);

            if (++Count != 6) return;

            ExitBecauseCancellation = false;
            throw new InvalidOperationException("Expected exception");
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