using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Transactions.Workflows
{
    public interface IActivity
    {
        void Execute();
        IActivity CompensateWith(Action action);
        IActivity CancelWith(Action action);
        bool Executed { get; }
        bool IsExecuting { get; }
    }
}
