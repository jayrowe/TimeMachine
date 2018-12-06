using System;

namespace TimeMachine
{
    /// <summary>
    /// Interface for a timer that can have its signalling parameters modified and
    /// can be disposed.
    /// </summary>
    public interface ITimer : IDisposable
    {
        /// <summary>
        /// Changes the start time and interval for the timer
        /// </summary>
        /// <param name="dueTime">
        /// Time before calling the callback after timer is created; specify -1 to prevent the timer from firing;
        /// specify 0 to fire immediately
        /// </param>
        /// <param name="period">
        /// Time interval between subsequents calls to the callback; specify -1 to prevent periodic signalling
        /// </param>
        /// <returns>
        /// True if the timer was updated, false otherwise
        /// </returns>
        bool Change(int dueTime, int period);

        /// <summary>
        /// Changes the start time and interval for the timer
        /// </summary>
        /// <param name="dueTime">
        /// Time before calling the callback after timer is created; specify -1 to prevent the timer from firing;
        /// specify 0 to fire immediately. Cannot be greater than 4294967294.
        /// </param>
        /// <param name="period">
        /// Time interval between subsequents calls to the callback; specify -1 to prevent periodic signalling.
        /// Cannot be greater than 4294967294.
        /// </param>
        /// <returns>
        /// True if the timer was updated, false otherwise
        /// </returns>
        bool Change(long dueTime, long period);

        /// <summary>
        /// Changes the start time and interval for the timer
        /// </summary>
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
        /// True if the timer was updated, false otherwise
        /// </returns>
        bool Change(TimeSpan dueTime, TimeSpan period);

        /// <summary>
        /// Changes the start time and interval for the timer
        /// </summary>
        /// <param name="dueTime">
        /// Time before calling the callback after timer is created; specify -1 to prevent the timer from firing;
        /// specify 0 to fire immediately.
        /// </param>
        /// <param name="period">
        /// Time interval between subsequents calls to the callback; specify a TimeSpan of -1 milliseconds to prevent
        /// periodic signalling.
        /// </param>
        /// <returns>
        /// True if the timer was updated, false otherwise
        /// </returns>
        bool Change(uint dueTime, uint period);
    }
}
