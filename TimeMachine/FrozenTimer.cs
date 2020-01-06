using System;
using System.Threading;

namespace TimeMachine
{
    internal class FrozenTimer : ITimer, IFrozen
    {
        private const long MAX_INTERVAL_MILLISECONDS = uint.MaxValue - 1;

        private readonly TimerCallback _callback;
        private readonly object _state;
        private long _period;
        private Timer _timer;
        private readonly object _lock = new object();

        public FrozenTimer(TimerCallback callback)
        {
            _callback = callback ?? throw new ArgumentNullException(
                nameof(callback),
                $"{nameof(callback)} cannot be null");
            _state = this;

            Change(-1, -1);
        }

        public FrozenTimer(TimerCallback callback, object state, int dueTime, int period)
        {
            _callback = callback ?? throw new ArgumentNullException(
                nameof(callback),
                $"{nameof(callback)} cannot be null");
            _state = state;

            Change(dueTime, period);
        }

        public FrozenTimer(TimerCallback callback, object state, long dueTime, long period)
        {
            _callback = callback ?? throw new ArgumentNullException(
                nameof(callback),
                $"{nameof(callback)} cannot be null");
            _state = state;

            Change(dueTime, period);
        }

        public FrozenTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            _callback = callback ?? throw new ArgumentNullException(
                nameof(callback),
                $"{nameof(callback)} cannot be null");
            _state = state;

            Change(dueTime, period);
        }

        public FrozenTimer(TimerCallback callback, object state, uint dueTime, uint period)
        {
            _callback = callback ?? throw new ArgumentNullException(
                nameof(callback),
                $"{nameof(callback)} cannot be null");
            _state = state;

            Change(dueTime, period);
        }

        public DateTime DueTime { get; private set; }

        public void Thaw()
        {
            lock (_lock)
            {
                if (DueTime == DateTime.MaxValue)
                {
                    _timer = new Timer(_callback, _state, -1, -1);
                    return;
                }

                var initial = (DueTime - TimeProvider.UtcNow).Ticks / TimeSpan.TicksPerMillisecond;

                if (initial < 0)
                {
                    initial = 0;
                }

                _timer = new Timer(_callback, _state, initial, _period);
            }
        }

        public bool Change(int dueTime, int period)
        {
            if (dueTime < -1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(dueTime),
                    $"{nameof(dueTime)} must be greater than or equal to -1");
            }

            if (period < -1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(period),
                    $"{nameof(period)} must be greater than or equal to -1");
            }

            return ChangeWithoutValidation(dueTime, period);
        }

        public bool Change(long dueTime, long period)
        {
            if (dueTime < -1 || dueTime > MAX_INTERVAL_MILLISECONDS)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(dueTime),
                    $"{nameof(dueTime)} must be greater than or equal to -1 and less than {uint.MaxValue}");
            }

            if (period < -1 || period > MAX_INTERVAL_MILLISECONDS)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(period),
                    $"{nameof(period)} must be greater than or equal to -1 and less than {uint.MaxValue}");
            }

            return ChangeWithoutValidation(dueTime, period);
        }

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            var dueTimeMillieconds = (long)dueTime.TotalMilliseconds;
            var periodMilliseconds = (long)period.TotalMilliseconds;

            if (dueTimeMillieconds < -1 || dueTimeMillieconds > MAX_INTERVAL_MILLISECONDS)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(dueTime),
                    $"{nameof(dueTime)} must be greater than or equal to -1 millisecond and less than {uint.MaxValue} milliseconds");
            }

            if (periodMilliseconds < -1 || periodMilliseconds > MAX_INTERVAL_MILLISECONDS)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(period),
                    $"{nameof(period)} must be greater than or equal to -1 millisecond and less than {uint.MaxValue} milliseconds");
            }

            return Change(dueTimeMillieconds, periodMilliseconds);
        }

        public bool Change(uint dueTime, uint period)
        {
            return ChangeWithoutValidation(
                dueTime == uint.MaxValue ? -1L : dueTime,
                period == uint.MaxValue ? -1L : period);
        }

        private bool ChangeWithoutValidation(long dueTime, long period)
        {
            lock (_lock)
            {
                if (_timer != null)
                {
                    return _timer.Change(dueTime, period);
                }

                _period = period;

                if (dueTime > -1)
                {
                    DueTime = TimeProvider.UtcNow.AddMilliseconds(dueTime);
                }
                else
                {
                    DueTime = DateTime.MaxValue;
                }
            }

            return true;
        }

        public bool Fire()
        {
            _callback(_state);

            AdvanceDueTime();

            return true;
        }

        private void AdvanceDueTime()
        {
            if (_period > 0)
            {
                DueTime = DueTime.AddMilliseconds(_period);
            }
            else
            {
                DueTime = DateTime.MaxValue;
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();

            TimeProvider.Unregister(this);
        }
    }
}
