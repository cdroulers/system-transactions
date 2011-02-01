using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Transactions.Test.Unit
{
    public interface ITestRepo<T>
    {
        void DoSomething(T value);
        void RevertSomething(T value);
        void CancelSomething(T value);
    }
}
