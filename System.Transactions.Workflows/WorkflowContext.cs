using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace System.Transactions.Workflows
{
    public class WorkflowContext : IDisposable
    {
        public bool Completed { get; private set; }
        public bool RolledBack { get; private set; }
        private bool _disposed;

        private readonly Collection<Activity> _activities = new Collection<Activity>();

        public void Complete()
        {
            if (this.Completed)
            {
                throw new InvalidOperationException(Properties.Strings.WorkflowAlreadyCompletedCannotCompleteAgain);
            }
            if (this.RolledBack)
            {
                throw new InvalidOperationException(Properties.Strings.WorkflowAlreadyRolledBackCannotComplete);
            }
            if (this._disposed)
            {
                throw new ObjectDisposedException(Properties.Strings.WorkflowDisposedCannotComplete);
            }

            this.Completed = true;

            // TODO: Execute complete stuff (is there any?)
        }

        public void RollBack()
        {
            if (this.Completed)
            {
                throw new InvalidOperationException(Properties.Strings.WorkflowAlreadyCompletedCannotRollBack);
            }
            if (this.RolledBack)
            {
                throw new InvalidOperationException(Properties.Strings.WorkflowAlreadyRolledBackCannotRollBackAgain);
            }
            if (this._disposed)
            {
                throw new ObjectDisposedException(Properties.Strings.WorkflowDisposedCannotRollBack);
            }

            foreach (var activity in this._activities)
            {
                if (activity.Executed)
                {
                    activity.Compensate();
                }
                else if (activity.IsExecuting)
                {
                    activity.Cancel();
                }
            }

            this.RolledBack = true;
        }

        public void Dispose()
        {
            try
            {
                if (!this.Completed)
                {
                    this.RollBack();
                }
            }
            finally
            {
                this._disposed = true;
            }
        }

        public IActivity Act(Action action)
        {
            var activity = new Activity(this, action);
            this._activities.Add(activity);
            return activity;
        }
    }
}
