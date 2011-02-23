using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace System.Transactions.Workflows.Test.Unit
{
    [TestClass]
    public class GivenNestedWorkflowContexts
    {
        private Mock<ITestRepo<int>> _moq;
        private ITestRepo<int> _moqObject;
        private WorkflowContext _context;

        [TestInitialize]
        public void TestInitialize()
        {
            _moq = new Mock<ITestRepo<int>>();
            _moqObject = _moq.Object;
            _context = new WorkflowContext();
        }

        // Write tests that check that make sure that rollbacking a parent workflow rolls back the child workflows.
        // Also think about adding a "Confirm" on both actions and workflows.
    }
}
