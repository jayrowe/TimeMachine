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

        public void Thaw()
        {
            var delay = DueTime - DateTime.UtcNow;

            if (delay <= TimeSpan.Zero)
            {
                Fire();
            }
            else
            {
                Task.Delay(delay).ContinueWith(t => Fire());
            }
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
