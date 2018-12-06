using System;

namespace TimeMachine
{
    public class Delorean : IDisposable
    {
        public void Freeze() => TimeProvider.Freeze();

        public void Thaw() => TimeProvider.Thaw();

        public void Advance(TimeSpan timeSpan) => TimeProvider.Advance(timeSpan);

        public void Advance(int milliseconds) => TimeProvider.Advance(TimeSpan.FromMilliseconds(milliseconds));

        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            Thaw();
        }
    }
}
