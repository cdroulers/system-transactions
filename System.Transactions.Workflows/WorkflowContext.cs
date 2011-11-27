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

        private readonly Collection<BaseActivity> _activities = new Collection<BaseActivity>();

        public void Complete()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(Properties.Strings.WorkflowDisposedCannotComplete);
            }
            if (this.Completed)
            {
                throw new InvalidOperationException(Properties.Strings.WorkflowAlreadyCompletedCannotCompleteAgain);
            }
            if (this.RolledBack)
            {
                throw new InvalidOperationException(Properties.Strings.WorkflowAlreadyRolledBackCannotComplete);
            }

            this.Completed = true;

            foreach (var activity in this._activities.Where(a => !a.Confirmed))
            {
                activity.Confirm();
            }
        }

        public void RollBack()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(Properties.Strings.WorkflowDisposedCannotRollBack);
            }
            if (this.Completed)
            {
                throw new InvalidOperationException(Properties.Strings.WorkflowAlreadyCompletedCannotRollBack);
            }
            if (this.RolledBack)
            {
                throw new InvalidOperationException(Properties.Strings.WorkflowAlreadyRolledBackCannotRollBackAgain);
            }

            foreach (var activity in this._activities.Where(a => !a.Confirmed).OrderByDescending(a => a.ExecutedOn))
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
                if (!this.Completed && !this.RolledBack)
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
            if (this._disposed)
            {
                throw new ObjectDisposedException(Properties.Strings.WorkflowDisposedCannotAct);
            }

            var activity = new Activity(this, action);
            this._activities.Add(activity);
            return activity;
        }

        public void Execute(Action action, Action compensateWith, Action cancelWith)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(Properties.Strings.WorkflowDisposedCannotExecute);
            }

            IActivity activity;
            this.Execute(action, compensateWith, cancelWith, out activity);
        }

        public void Execute(Action action, Action compensateWith, Action cancelWith, out IActivity activity)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(Properties.Strings.WorkflowDisposedCannotExecute);
            }

            activity = this.Act(action);
            if (compensateWith != null)
            {
                activity.CompensateWith(compensateWith);
            }
            if (cancelWith != null)
            {
                activity.CancelWith(cancelWith);
            }
            activity.Execute();
        }

        public IActivityWithResult<T> Act<T>(Func<T> action)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(Properties.Strings.WorkflowDisposedCannotAct);
            }

            var activity = new ActivityWithResult<T>(this, action);
            this._activities.Add(activity);
            return activity;
        }

        public T Execute<T>(Func<T> action, Action<T> compensateWith, Action cancelWith)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(Properties.Strings.WorkflowDisposedCannotExecute);
            }

            IActivityWithResult<T> activity;
            return this.Execute<T>(action, compensateWith, cancelWith, out activity);
        }

        public T Execute<T>(Func<T> action, Action<T> compensateWith, Action cancelWith, out IActivityWithResult<T> activity)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(Properties.Strings.WorkflowDisposedCannotExecute);
            }

            activity = this.Act(action);
            if (compensateWith != null)
            {
                activity.CompensateWith(compensateWith);
            }
            if (cancelWith != null)
            {
                activity.CancelWith(cancelWith);
            }
            return activity.Execute();
        }

        public void ForEach<T>(IEnumerable<T> items, Action<T> action)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(Properties.Strings.WorkflowDisposedCannotForEach);
            }

            foreach (var item in items)
            {
                action(item);
            }
        }
    }
}
