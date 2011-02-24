using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Transactions.Workflows
{
    public interface IActivityWithResult<T>
    {
        T Execute();
        IActivityWithResult<T> CompensateWith(Action<T> action);
        IActivityWithResult<T> CancelWith(Action action);
        bool Executed { get; }
        bool IsExecuting { get; }

        void Confirm();
        bool Confirmed { get; }
    }
}
