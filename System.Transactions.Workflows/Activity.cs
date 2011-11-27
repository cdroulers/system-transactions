using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace System.Transactions.Workflows
{
    public class Activity : BaseActivity, IActivity
    {
        public Action Action { get; private set; }

        public Action Compensation { get; private set; }

        public Action Cancellation { get; private set; }

        internal Activity(WorkflowContext context, Action action)
            : base(context)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            this.Action = action;
        }

        public void Execute()
        {
            this.IsExecuting = true;
            this.Action();
            this.IsExecuting = false;
            this.ExecutedOn = DateTime.UtcNow;
        }

        public IActivity CompensateWith(Action action)
        {
            if (this.Compensation != null)
            {
                throw new InvalidOperationException(Properties.Strings.CannotCompensateMoreThanOnce);
            }
            this.Compensation = action;
            return this;
        }

        public IActivity CancelWith(Action action)
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
                this.Compensation();
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
