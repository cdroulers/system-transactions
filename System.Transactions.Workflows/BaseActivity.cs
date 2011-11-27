using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace System.Transactions.Workflows
{
    public abstract class BaseActivity
    {
        public WorkflowContext Context { get; private set; }

        public bool Executed { get { return this.ExecutedOn.HasValue; } }
        public DateTime? ExecutedOn { get; protected set; }

        public bool IsExecuting { get; protected set; }

        internal BaseActivity(WorkflowContext context)
        {
            this.Context = context;
        }

        internal abstract void Compensate();
        internal abstract void Cancel();

        public void Confirm()
        {
            this.Confirmed = true;
        }

        public bool Confirmed { get; private set; }
    }
}
