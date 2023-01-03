using System;
using System.Threading;
using System.Threading.Tasks;

namespace TimeMachine
{
    internal class FrozenDelay : IFrozen
    {
        private readonly TaskCompletionSource<bool> _completionSource;

        public FrozenDelay(DateTime dueTime, CancellationToken cancellationToken)
        {
            DueTime = dueTime;

            _completionSource = new TaskCompletionSource<bool>();

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(TryCancel);
            }
        }

        public DateTime DueTime { get; }

        public Task Task => _completionSource.Task;

        public async void Thaw()
        {
            // Time could have been moved forward and then a delay set such that we'd exceed int.MaxValue
            // to thaw this delay and allow it to continue. Call Task.Delay repeatedly to reach the desired
            // delay.
            long delay;

            do
            {
                delay = Math.Max(0, (DueTime - DateTime.UtcNow).Ticks / TimeSpan.TicksPerMillisecond);
                await Task.Delay((int)Math.Min(int.MaxValue, delay));
            }
            while (delay > 0);

            Fire();
        }

        public bool Fire()
        {
            _completionSource.TrySetResult(true);
            return false;
        }

        private void TryCancel()
        {
            _completionSource.TrySetCanceled();

            TimeProvider.Unregister(this);
        }
    }
}
