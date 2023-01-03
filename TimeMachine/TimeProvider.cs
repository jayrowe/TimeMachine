using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TimeMachine
{
    /// <summary>
    /// Provides the current time and supports delays in a way that can be manipulated during tests
    /// </summary>
    public static class TimeProvider
    {
        private static bool _frozen;
        private static DateTime _staticNow;

        private static readonly object _lock = new object();
        private static LinkedList<IFrozen> _callbacks = new LinkedList<IFrozen>();

        internal static void Freeze()
        {
            lock (_lock)
            {
                Freeze(UtcNow);
            }
        }

        internal static void Freeze(DateTime staticNow)
        {
            lock (_lock)
            {
                _staticNow = staticNow;
                _frozen = true;
            }
        }

        internal static void Thaw()
        {
            lock (_lock)
            {
                var next = _callbacks.First;

                while (next != null)
                {
                    var current = next;
                    next = next.Next;

                    current.Value.Thaw();
                }

                _callbacks.Clear();

                _frozen = false;
            }
        }

        internal static void Advance(TimeSpan time)
        {
            lock (_lock)
            {
                if (!_frozen)
                {
                    throw new InvalidOperationException("Cannot advance time when time is not frozen");
                }

                _staticNow = UtcNow + time;

                FireCallbacks();
            }
        }

        private static void FireCallbacks()
        {
            var next = _callbacks.First;

            while (next != null && next.Value.DueTime <= _staticNow)
            {
                // re-insert the timer
                _callbacks.Remove(next);

                if (next.Value.Fire())
                {
                    Register(next);
                }

                next = _callbacks.First;
            }
        }

        internal static void Register(IFrozen callback)
        {
            Register(new LinkedListNode<IFrozen>(callback));
        }

        internal static void Unregister(IFrozen callback)
        {
            lock (_lock)
            {
                _callbacks.Remove(callback);
            }
        }

        private static void Register(LinkedListNode<IFrozen> insert)
        {
            LinkedListNode<IFrozen> previous = null;
            var current = _callbacks.First;

            while (current != null && current.Value.DueTime <= insert.Value.DueTime)
            {
                previous = current;
                current = current.Next;
            }

            if (previous == null)
            {
                _callbacks.AddFirst(insert);
            }
            else
            {
                _callbacks.AddAfter(previous, insert);
            }
        }

        /// <summary>
        /// Fetches the current time or a static time value if time is currently frozen
        /// </summary>
        public static DateTime UtcNow => _frozen ? _staticNow : DateTime.UtcNow;

        /// <summary>
        /// Fetches the current time or a static time value if time is currently frozen
        /// </summary>
        public static DateTimeOffset UtcNowOffset => UtcNow;

        /// <summary>
        /// Returns a task that will return after the specified delay, subject to the time flow
        /// constraints enforced at the time this method is called.
        /// </summary>
        /// <param name="millisecondsDelay">
        /// Desired delay
        /// </param>
        /// <returns>
        /// Task that will return after the specified delay
        /// </returns>
        public static Task Delay(int millisecondsDelay) => Delay(millisecondsDelay, default);

        /// <summary>
        /// Returns a task that will return after the specified delay, subject to the time flow
        /// constraints enforced at the time this method is called.
        /// </summary>
        /// <param name="millisecondsDelay">
        /// Desired delay
        /// </param>
        /// <param name="cancellationToken">
        /// <see cref="CancellationToken"/> that can be used to cancel the delay
        /// </param>
        /// <returns>
        /// Task that will return after the specified delay
        /// </returns>
        public static Task Delay(int millisecondsDelay, CancellationToken cancellationToken)
        {
            if (_frozen)
            {
                lock (_lock)
                {
                    if (_frozen)
                    {
                        return AddDelay(millisecondsDelay, cancellationToken);
                    }
                }
            }

            return Task.Delay(millisecondsDelay, cancellationToken);
        }

        /// <summary>
        /// Returns a task that will return after the specified delay, subject to the time flow
        /// constraints enforced at the time this method is called.
        /// </summary>
        /// <param name="delay">
        /// Desired delay
        /// </param>
        /// <returns>
        /// Task that will return after the specified delay
        /// </returns>
        public static Task Delay(TimeSpan delay) => Delay(delay, default);

        /// <summary>
        /// Returns a task that will return after the specified delay, subject to the time flow
        /// constraints enforced at the time this method is called.
        /// </summary>
        /// <param name="delay">
        /// Desired delay
        /// </param>
        /// <param name="cancellationToken">
        /// <see cref="CancellationToken"/> that can be used to cancel the delay
        /// </param>
        /// <returns>
        /// Task that will return after the specified delay
        /// </returns>
        public static Task Delay(TimeSpan delay, CancellationToken cancellationToken)
        {
            if (_frozen)
            {
                lock (_lock)
                {
                    if (_frozen)
                    {
                        var millisecondsDelay = (long)delay.TotalMilliseconds;

                        if (millisecondsDelay < -1 || millisecondsDelay > int.MaxValue)
                        {
                            throw new ArgumentOutOfRangeException(
                                nameof(delay),
                                $"{nameof(delay)} must be less than or equal to {int.MaxValue} milliseconds and greater than or equal to -1 millisecond");
                        }

                        return AddDelay((int) millisecondsDelay, cancellationToken);
                    }
                }
            }

            return Task.Delay(delay, cancellationToken);
        }

        private static Task AddDelay(int millisecondsDelay, CancellationToken cancellationToken)
        {
            if (millisecondsDelay < -1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(millisecondsDelay),
                    $"{nameof(millisecondsDelay)} must be greater than or equal to -1");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            if (millisecondsDelay == 0)
            {
                return Task.CompletedTask;
            }

            if (millisecondsDelay == -1)
            {
                // nothing better we can do here
                return Task.Delay(-1, cancellationToken);
            }

            var callback = new FrozenDelay(UtcNow.AddMilliseconds(millisecondsDelay), cancellationToken);
            Register(callback);
            return callback.Task;
        }

        /// <summary>
        /// Starts a new timer with signalling disabled, using the created timer as the state
        /// </summary>
        /// <param name="callback">
        /// Callback to be executed when the timer fires
        /// </param>
        /// <returns>
        /// A timer
        /// </returns>
        public static ITimer Timer(TimerCallback callback)
        {
            if (_frozen)
            {
                lock (_lock)
                {
                    if (_frozen)
                    {
                        var timer = new FrozenTimer(callback);
                        Register(timer);
                        return timer;
                    }
                }
            }

            return new ThawedTimer(callback);
        }

        /// <summary>
        /// Starts a new timer with the given state and signalling configuration
        /// </summary>
        /// <param name="callback">
        /// Callback to be executed when the timer fires
        /// </param>
        /// <param name="state">
        /// State object to be passed to the callback when the timer fires
        /// </param>
        /// <param name="dueTime">
        /// Time before calling the callback after timer is created; specify -1 to prevent the timer from firing;
        /// specify 0 to fire immediately
        /// </param>
        /// <param name="period">
        /// Time interval between subsequents calls to the callback; specify -1 to prevent periodic signalling
        /// </param>
        /// <returns>
        /// A timer
        /// </returns>
        public static ITimer Timer(TimerCallback callback, object state, int dueTime, int period)
        {
            if (_frozen)
            {
                lock (_lock)
                {
                    if (_frozen)
                    {
                        var timer = new FrozenTimer(callback, state, dueTime, period);
                        Register(timer);
                        return timer;
                    }
                }
            }

            return new ThawedTimer(callback, state, dueTime, period);
        }

        /// <summary>
        /// Starts a new timer with the given state and signalling configuration
        /// </summary>
        /// <param name="callback">
        /// Callback to be executed when the timer fires
        /// </param>
        /// <param name="state">
        /// State object to be passed to the callback when the timer fires
        /// </param>
        /// <param name="dueTime">
        /// Time before calling the callback after timer is created; specify -1 to prevent the timer from firing;
        /// specify 0 to fire immediately. Cannot be greater than 4294967294.
        /// </param>
        /// <param name="period">
        /// Time interval between subsequents calls to the callback; specify -1 to prevent periodic signalling.
        /// Cannot be greater than 4294967294.
        /// </param>
        /// <returns>
        /// A timer
        /// </returns>
        public static ITimer Timer(TimerCallback callback, object state, long dueTime, long period)
        {
            if (_frozen)
            {
                lock (_lock)
                {
                    if (_frozen)
                    {
                        var timer = new FrozenTimer(callback, state, dueTime, period);
                        Register(timer);
                        return timer;
                    }
                }
            }

            return new ThawedTimer(callback, state, dueTime, period);
        }

        /// <summary>
        /// Starts a new timer with the given state and signalling configuration
        /// </summary>
        /// <param name="callback">
        /// Callback to be executed when the timer fires
        /// </param>
        /// <param name="state">
        /// State object to be passed to the callback when the timer fires
        /// </param>
        /// <param name="dueTime">
        /// Time before calling the callback after timer is created; specify a TimeSpan of -1 milliseconds to prevent
        /// the timer from firing; specify <see cref="TimeSpan.Zero"/> to fire immediately. TimeSpan cannot be greater
        /// than 4294967294 milliseconds.
        /// </param>
        /// <param name="period">
        /// Time interval between subsequents calls to the callback; specify a TimeSpan of -1 milliseconds to prevent
        /// periodic signalling. TimeSpan cannot be greater than 4294967294 milliseconds.
        /// </param>
        /// <returns>
        /// A timer
        /// </returns>
        public static ITimer Timer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            if (_frozen)
            {
                lock (_lock)
                {
                    if (_frozen)
                    {
                        var timer = new FrozenTimer(callback, state, dueTime, period);
                        Register(timer);
                        return timer;
                    }
                }
            }

            return new ThawedTimer(callback, state, dueTime, period);
        }

        /// <summary>
        /// Starts a new timer with the given state and signalling configuration
        /// </summary>
        /// <param name="callback">
        /// Callback to be executed when the timer fires
        /// </param>
        /// <param name="state">
        /// State object to be passed to the callback when the timer fires
        /// </param>
        /// <param name="dueTime">
        /// Time before calling the callback after timer is created; specify -1 to prevent the timer from firing;
        /// specify 0 to fire immediately.
        /// </param>
        /// <param name="period">
        /// Time interval between subsequents calls to the callback; specify a TimeSpan of -1 milliseconds to prevent
        /// periodic signalling.
        /// </param>
        /// <returns>
        /// A timer
        /// </returns>
        public static ITimer Timer(TimerCallback callback, object state, uint dueTime, uint period)
        {
            if (_frozen)
            {
                lock (_lock)
                {
                    if (_frozen)
                    {
                        var timer = new FrozenTimer(callback, state, dueTime, period);
                        Register(timer);
                        return timer;
                    }
                }
            }

            return new ThawedTimer(callback, state, dueTime, period);
        }   
    }
}
