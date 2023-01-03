using System;
using System.Threading;

namespace TimeMachine
{
    public sealed class Delorean : IDisposable
    {
        private static Delorean _current;

        public Delorean()
            : this(true)
        {
        }

        public Delorean(bool freeze)
        {
            if (Interlocked.CompareExchange(ref _current, this, null) != null)
            {
                throw new InvalidOperationException(
                    "Cannot run competing Deloreans");
            }

            if (freeze)
            {
                Freeze();
            }
        }

        public void Freeze() => TimeProvider.Freeze();

        public void Freeze(DateTime staticNow) => TimeProvider.Freeze(staticNow);

        public void Thaw() => TimeProvider.Thaw();

        public void Advance(TimeSpan timeSpan) => TimeProvider.Advance(timeSpan);

        public void Advance(int milliseconds) => TimeProvider.Advance(TimeSpan.FromMilliseconds(milliseconds));

        public void Dispose()
        {
            if (_current == this)
            {
                Thaw();
            }
            else
            {
                throw new InvalidOperationException();
            }

            Interlocked.CompareExchange(ref _current, null, this);
        }
    }
}
