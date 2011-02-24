using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace System.Transactions.Workflows
{
    public class ActivityWithResult<T> : BaseActivity, IActivityWithResult<T>
    {
        public Func<T> Action { get; private set; }

        public Action<T> Compensation { get; private set; }

        public Action Cancellation { get; private set; }

        private T Result { get; set; }

        internal ActivityWithResult(WorkflowContext context, Func<T> action)
            : base(context)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            this.Action = action;
        }

        public T Execute()
        {
            this.IsExecuting = true;
            this.Result = this.Action();
            this.IsExecuting = false;
            this.Executed = true;
            return this.Result;
        }

        public IActivityWithResult<T> CompensateWith(Action<T> action)
        {
            if (this.Compensation != null)
            {
                throw new InvalidOperationException(Properties.Strings.CannotCompensateMoreThanOnce);
            }
            this.Compensation = action;
            return this;
        }

        public IActivityWithResult<T> CancelWith(Action action)
        {
            if (this.Cancellation != null)
            {
                throw new InvalidOperationException(Properties.Strings.CannotCancelMoreThanOnce);
            }
            this.Cancellation = action;
            return this;
        }

        internal override void Compensate()
        {
            if (this.Compensation != null)
            {
                this.Compensation(this.Result);
            }
        }

        internal override void Cancel()
        {
            if (this.Cancellation != null)
            {
                this.Cancellation();
            }
        }
    }
}
