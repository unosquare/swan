namespace Unosquare.Swan.Test.Mocks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;

    public class AppWorkerMock : TimerWorkerBase
    {
        public AppWorkerMock()
            : base(nameof(AppWorkerMock), TimeSpan.FromMilliseconds(100))
        {
        }

        public Exception Exception { get; private set; }
        public bool ExitBecauseCancellation { get; private set; } = true;
        public int Count { get; private set; }

        public Action OnExit { get; set; }

        public override Task<WorkerState> StopAsync()
        {
            OnExit?.Invoke();

            return base.StopAsync();
        }

        protected override void OnCycleException(Exception ex)
        {
            Exception = ex;
        }

        protected override void ExecuteCycleLogic(CancellationToken ct)
        {
            if (++Count != 6) return;

            ExitBecauseCancellation = false;
            throw new InvalidOperationException("Expected exception");
        }

        protected override void OnDisposing()
        {
            // do nothing
        }
    }
}