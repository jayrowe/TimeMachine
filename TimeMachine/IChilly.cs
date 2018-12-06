using System;

namespace TimeMachine
{
    internal interface IChilly
    {
        DateTime DueTime { get; }
        bool Fire();
        void Thaw();
    }
}
