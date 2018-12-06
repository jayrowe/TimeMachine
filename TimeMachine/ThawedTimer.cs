using System;
using System.Threading;

namespace TimeMachine
{
    internal class ThawedTimer : ITimer
    {
        private readonly Timer _timer;

        public ThawedTimer(TimerCallback callback)
            => _timer = new Timer(callback);
        public ThawedTimer(TimerCallback callback, object state, int dueTime, int period)
            => _timer = new Timer(callback, state, dueTime, period);
        public ThawedTimer(TimerCallback callback, object state, long dueTime, long period)
            => _timer = new Timer(callback, state, dueTime, period);
        public ThawedTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
            => _timer = new Timer(callback, state, dueTime, period);
        public ThawedTimer(TimerCallback callback, object state, uint dueTime, uint period)
            => _timer = new Timer(callback, state, dueTime, period);

        public bool Change(int dueTime, int period)
            => _timer.Change(dueTime, period);

        public bool Change(long dueTime, long period)
            => _timer.Change(dueTime, period);

        public bool Change(TimeSpan dueTime, TimeSpan period)
            => _timer.Change(dueTime, period);

        public bool Change(uint dueTime, uint period)
            => _timer.Change(dueTime, period);

        public void Dispose() => _timer.Dispose();
    }
}
