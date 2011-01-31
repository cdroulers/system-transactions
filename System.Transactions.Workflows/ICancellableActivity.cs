using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Transactions
{
    public interface ICancellableActivity
    {
        IActivity Cancel(Action action);
        void ExecuteCancellation();
    }
}
