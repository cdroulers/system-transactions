using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions.Activities;

namespace System.Transactions
{
    public struct WorkflowContext : IDisposable
    {
        public bool Completed { get; private set; }
        private bool _disposed;

        public void Complete()
        {
            if (this.Completed)
            {
                throw new InvalidOperationException(Properties.Strings.WorkflowAlreadyCompletedCannotCompleteAgain);
            }
            if (this._disposed)
            {
                throw new ObjectDisposedException(Properties.Strings.WorkflowDisposedCannotComplete);
            }

            this.Completed = true;

            // TODO: Execute complete stuff (is there any?)
        }

        public void Rollback()
        {
            if (this.Completed)
            {
                throw new InvalidOperationException(Properties.Strings.WorkflowAlreadyCompletedCannotRollback);
            }
            if (this._disposed)
            {
                throw new ObjectDisposedException(Properties.Strings.WorkflowDisposedCannotRollback);
            }

            // TODO: Execute rollback stuff
        }

        public void Dispose()
        {
            try
            {
                if (!this.Completed)
                {
                    this.Rollback();
                }
            }
            finally
            {
                this._disposed = true;
            }
        }

        public IActivity Act(Action action)
        {
            return new Activity(this, action);
        }
    }
}
