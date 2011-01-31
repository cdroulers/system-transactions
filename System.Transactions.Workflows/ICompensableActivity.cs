using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Transactions
{
    public interface ICompensableActivity
    {
        IActivity Compensate(Action action);
        void ExecuteCompensation();
    }
}
