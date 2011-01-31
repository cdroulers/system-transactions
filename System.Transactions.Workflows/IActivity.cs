using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Transactions
{
    public interface IActivity : ICompensableActivity, ICancellableActivity
    {
        void Execute();
        bool Executed { get; }
        bool IsExecuting { get; }
    }
}
