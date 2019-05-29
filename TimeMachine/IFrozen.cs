using System;

namespace TimeMachine
{
    internal interface IFrozen
    {
        DateTime DueTime { get; }
        bool Fire();
        void Thaw();
    }
}
