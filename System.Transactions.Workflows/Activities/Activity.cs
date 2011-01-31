using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace System.Transactions.Activities
{
    public class Activity : IActivity
    {
        public WorkflowContext Context { get; private set; }

        public Action Action { get; private set; }

        public Action Compensation { get; private set; }

        public Action Cancellation { get; private set; }

        public bool Executed { get; private set; }

        public bool IsExecuting { get; private set; }

        public Activity(WorkflowContext context, Action action)
        {
            this.Context = context;
            this.Action = action;
        }

        public void Execute()
        {
            this.IsExecuting = true;
            this.Action();
            this.IsExecuting = false;
            this.Executed = true;
        }

        public IActivity Compensate(Action action)
        {
            if (this.Compensation != null)
            {
                throw new InvalidOperationException(Properties.Strings.CannotCompensateMoreThanOnce);
            }
            this.Compensation = action;
            return this;
        }

        public IActivity Cancel(Action action)
        {
            if (this.Cancellation != null)
            {
                throw new InvalidOperationException(Properties.Strings.CannotCancelMoreThanOnce);
            }
            this.Cancellation = action;
            return this;
        }

        void ICompensableActivity.ExecuteCompensation()
        {
            if (this.Compensation != null)
            {
                this.Compensation();
            }
        }

        void ICancellableActivity.ExecuteCancellation()
        {
            if (this.Cancellation != null)
            {
                this.Cancellation();
            }
        }
    }
}
