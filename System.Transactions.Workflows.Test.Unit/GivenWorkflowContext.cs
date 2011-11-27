using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace System.Transactions.Workflows.Test.Unit
{
    // ReSharper disable InconsistentNaming
    [TestClass]
    public class GivenWorkflowContext
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

        [TestMethod]
        public void When_Doing_Nothing_Then_Rolls_Back()
        {
            using (_context)
            {

            }

            Assert.IsTrue(_context.RolledBack);
            Assert.IsFalse(_context.Completed);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void When_Completing_Twice_Then_Fail()
        {
            using (_context)
            {
                _context.Complete();
                _context.Complete();
            }
        }

        [TestMethod]
        public void When_RollBacking_Manually_Then_All_Is_Well()
        {
            using (_context)
            {
                _context.RollBack();
            }

            Assert.IsTrue(_context.RolledBack);
            Assert.IsFalse(_context.Completed);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void When_RollBacking_Twice_Then_Fail()
        {
            using (_context)
            {
                _context.RollBack();
                _context.RollBack();
            }
        }

        [TestMethod]
        public void When_Calling_Complete_Then_Is_Completed()
        {
            using (_context)
            {
                _context.Complete();
            }

            Assert.IsTrue(_context.Completed);
            Assert.IsFalse(_context.RolledBack);
        }

        [TestMethod]
        public void When_Calling_Act_But_Not_Execute_Then_Nothing_Happens()
        {
            using (_context)
            {
                _context.Act(() => _moqObject.DoSomething(1));

                _context.Complete();
            }

            Assert.IsTrue(_context.Completed);
            Assert.IsFalse(_context.RolledBack);
            _moq.Verify(m => m.DoSomething(It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void When_Calling_Act_And_CompensateWith_But_Not_Execute_Then_Nothing_Happens()
        {
            using (_context)
            {
                _context.Act(() => _moqObject.DoSomething(1)).CompensateWith(() => _moqObject.RevertSomething(1));

                _context.Complete();
            }

            Assert.IsTrue(_context.Completed);
            Assert.IsFalse(_context.RolledBack);
            _moq.Verify(m => m.DoSomething(It.IsAny<int>()), Times.Never());
            _moq.Verify(m => m.RevertSomething(It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void When_Calling_Act_And_CompensateWith_And_CancelWith_But_Not_Execute_Then_Nothing_Happens()
        {
            using (_context)
            {
                _context.Act(() => _moqObject.DoSomething(1)).CompensateWith(() => _moqObject.RevertSomething(1)).CancelWith(() => _moqObject.CancelSomething(1));

                _context.Complete();
            }

            Assert.IsTrue(_context.Completed);
            Assert.IsFalse(_context.RolledBack);
            _moq.Verify(m => m.DoSomething(It.IsAny<int>()), Times.Never());
            _moq.Verify(m => m.RevertSomething(It.IsAny<int>()), Times.Never());
            _moq.Verify(m => m.CancelSomething(It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void When_Calling_Act_And_Execute_Then_Action_Is_Called()
        {
            using (_context)
            {
                _context.Act(() => _moqObject.DoSomething(1)).Execute();

                _context.Complete();
            }

            Assert.IsTrue(_context.Completed);
            Assert.IsFalse(_context.RolledBack);
            _moq.Verify(m => m.DoSomething(It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public void When_Calling_Act_And_CompensateWith_Execute_Then_Action_Is_Called_But_Not_Compensate()
        {
            using (_context)
            {
                _context.Act(() => _moqObject.DoSomething(1)).CompensateWith(() => _moqObject.RevertSomething(1)).Execute();

                _context.Complete();
            }

            Assert.IsTrue(_context.Completed);
            Assert.IsFalse(_context.RolledBack);
            _moq.Verify(m => m.DoSomething(It.IsAny<int>()), Times.Once());
            _moq.Verify(m => m.RevertSomething(It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void When_Calling_Act_And_CompensateWith_Execute_But_Not_Completed_Then_Action_Is_Called_And_Compensated()
        {
            using (_context)
            {
                _context.Act(() => _moqObject.DoSomething(1)).CompensateWith(() => _moqObject.RevertSomething(1)).Execute();
            }

            Assert.IsFalse(_context.Completed);
            Assert.IsTrue(_context.RolledBack);
            _moq.Verify(m => m.DoSomething(It.IsAny<int>()), Times.Once());
            _moq.Verify(m => m.RevertSomething(It.IsAny<int>()), Times.Once());
        }

        private const string ExceptionMessage = "Exception message";
        [TestMethod]
        public void When_Calling_Act_And_CompensateWith_Execute_But_Exception_After_Then_Action_Is_Called_And_Compensated()
        {
            try
            {
                using (_context)
                {
                    _context.Act(() => _moqObject.DoSomething(1)).CompensateWith(() => _moqObject.RevertSomething(1)).Execute();

                    throw new ApplicationException(ExceptionMessage);
                }
            }
            catch (ApplicationException e)
            {
                Assert.AreEqual(ExceptionMessage, e.Message);
            }

            Assert.IsFalse(_context.Completed);
            Assert.IsTrue(_context.RolledBack);
            _moq.Verify(m => m.DoSomething(It.IsAny<int>()), Times.Once());
            _moq.Verify(m => m.RevertSomething(It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public void When_Calling_Act_And_CancelWith_Execute_But_Exception_During_Then_Action_Is_Called_And_Cancelled()
        {
            _moq.Setup(m => m.DoSomething(It.IsAny<int>())).Throws(new ApplicationException(ExceptionMessage));
            try
            {
                using (_context)
                {
                    _context.Act(() => _moqObject.DoSomething(1)).CancelWith(() => _moqObject.CancelSomething(1)).Execute();
                }
            }
            catch (ApplicationException e)
            {
                Assert.AreEqual(ExceptionMessage, e.Message);
            }

            Assert.IsFalse(_context.Completed);
            Assert.IsTrue(_context.RolledBack);
            _moq.Verify(m => m.DoSomething(It.IsAny<int>()), Times.Once());
            _moq.Verify(m => m.CancelSomething(It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public void When_Calling_Act_And_CompensateWith_And_CancelWith_Execute_But_Exception_During_Then_Action_Is_Called_And_Cancelled_And_First_Is_Compensated()
        {
            _moq.Setup(m => m.DoSomething(2)).Throws(new ApplicationException(ExceptionMessage));
            try
            {
                using (_context)
                {
                    _context.Act(() => _moqObject.DoSomething(1)).CompensateWith(() => _moqObject.RevertSomething(1)).Execute();

                    _context.Act(() => _moqObject.DoSomething(2)).CancelWith(() => _moqObject.CancelSomething(2)).Execute();
                }
            }
            catch (ApplicationException e)
            {
                Assert.AreEqual(ExceptionMessage, e.Message);
            }

            Assert.IsFalse(_context.Completed);
            Assert.IsTrue(_context.RolledBack);
            _moq.Verify(m => m.DoSomething(1), Times.Once());
            _moq.Verify(m => m.DoSomething(2), Times.Once());
            _moq.Verify(m => m.RevertSomething(1), Times.Once());
            _moq.Verify(m => m.CancelSomething(2), Times.Once());
        }

        [TestMethod]
        public void When_Calling_Act_And_CompensateWith_And_CancelWith_Execute_But_Exception_During_Then_Action_Is_Called_And_Cancelled_And_First_Is_Compensated_And_Last_Is_Never_Called()
        {
            _moq.Setup(m => m.DoSomething(2)).Throws(new ApplicationException(ExceptionMessage));
            try
            {
                using (_context)
                {
                    _context.Act(() => _moqObject.DoSomething(1)).CompensateWith(() => _moqObject.RevertSomething(1)).Execute();

                    _context.Act(() => _moqObject.DoSomething(2)).CancelWith(() => _moqObject.CancelSomething(2)).Execute();

                    _moqObject.DoSomething(3);
                }
            }
            catch (ApplicationException e)
            {
                Assert.AreEqual(ExceptionMessage, e.Message);
            }

            Assert.IsFalse(_context.Completed);
            Assert.IsTrue(_context.RolledBack);
            _moq.Verify(m => m.DoSomething(1), Times.Once());
            _moq.Verify(m => m.DoSomething(2), Times.Once());
            _moq.Verify(m => m.RevertSomething(1), Times.Once());
            _moq.Verify(m => m.CancelSomething(2), Times.Once());
            _moq.Verify(m => m.DoSomething(3), Times.Never());
        }

        [TestMethod]
        public void When_Calling_Act_And_CompensateWith_And_CancelWith_Execute_Then_Action_Is_Called_And_Neither_Compensated_Nor_Cancelled()
        {
            using (_context)
            {
                _context.Act(() => _moqObject.DoSomething(1)).CompensateWith(() => _moqObject.RevertSomething(1)).Execute();

                _context.Act(() => _moqObject.DoSomething(2)).CancelWith(() => _moqObject.CancelSomething(2)).Execute();

                _moqObject.DoSomething(3);

                _context.Complete();
            }

            Assert.IsTrue(_context.Completed);
            Assert.IsFalse(_context.RolledBack);
            _moq.Verify(m => m.DoSomething(1), Times.Once());
            _moq.Verify(m => m.DoSomething(2), Times.Once());
            _moq.Verify(m => m.RevertSomething(1), Times.Never());
            _moq.Verify(m => m.CancelSomething(2), Times.Never());
            _moq.Verify(m => m.DoSomething(3), Times.Once());
        }

        [TestMethod]
        public void When_Acting_And_Manually_Rolling_Back_Then_Action_Are_Rolled_Back()
        {
            using (_context)
            {
                _context.Act(() => _moqObject.DoSomething(1)).CompensateWith(() => _moqObject.RevertSomething(1)).Execute();

                _context.Act(() => _moqObject.DoSomething(2)).CancelWith(() => _moqObject.CancelSomething(2)).Execute();

                _moqObject.DoSomething(3);

                _context.RollBack();
            }

            Assert.IsFalse(_context.Completed);
            Assert.IsTrue(_context.RolledBack);
            _moq.Verify(m => m.DoSomething(1), Times.Once());
            _moq.Verify(m => m.DoSomething(2), Times.Once());
            _moq.Verify(m => m.RevertSomething(1), Times.Once());
            _moq.Verify(m => m.CancelSomething(2), Times.Never());
            _moq.Verify(m => m.DoSomething(3), Times.Once());
        }

        [TestMethod]
        public void When_Confirming_And_Throws_Then_Action_Is_Not_Rolled_Back()
        {
            _moq.Setup(m => m.DoSomething(3)).Throws(new ApplicationException(ExceptionMessage));

            IActivity activity = null;
            IActivity activity2 = null;
            try
            {
                using (_context)
                {
                    activity = _context.Act(() => _moqObject.DoSomething(1)).CompensateWith(() => _moqObject.RevertSomething(1));

                    activity.Execute();

                    activity2 = _context.Act(() => _moqObject.DoSomething(2)).CancelWith(() => _moqObject.CancelSomething(2));
                    activity2.Execute();

                    activity.Confirm();

                    _moqObject.DoSomething(3);
                }
            }
            catch (Exception e)
            {
                Assert.AreEqual(ExceptionMessage, e.Message);
            }

            Assert.IsFalse(_context.Completed);
            Assert.IsTrue(_context.RolledBack);
            Assert.IsTrue(activity.Confirmed);
            Assert.IsFalse(activity2.Confirmed);
            _moq.Verify(m => m.DoSomething(1), Times.Once());
            _moq.Verify(m => m.DoSomething(2), Times.Once());
            _moq.Verify(m => m.RevertSomething(1), Times.Never());
            _moq.Verify(m => m.CancelSomething(2), Times.Never());
            _moq.Verify(m => m.DoSomething(3), Times.Once());
        }

        [TestMethod]
        public void When_Completing_Then_Activities_Are_All_Confirmed()
        {
            IActivity activity = null;
            IActivity activity2 = null;

            using (_context)
            {
                activity = _context.Act(() => _moqObject.DoSomething(1)).CompensateWith(() => _moqObject.RevertSomething(1));
                activity.Execute();

                activity2 = _context.Act(() => _moqObject.DoSomething(2)).CancelWith(() => _moqObject.CancelSomething(2));
                activity2.Execute();

                _context.Complete();
            }

            Assert.IsTrue(_context.Completed);
            Assert.IsFalse(_context.RolledBack);
            Assert.IsTrue(activity.Confirmed);
            Assert.IsTrue(activity2.Confirmed);
            _moq.Verify(m => m.DoSomething(1), Times.Once());
            _moq.Verify(m => m.DoSomething(2), Times.Once());
            _moq.Verify(m => m.RevertSomething(1), Times.Never());
            _moq.Verify(m => m.CancelSomething(2), Times.Never());
            _moq.Verify(m => m.DoSomething(3), Times.Never());
        }

        [TestMethod]
        public void When_Rollbacking_Then_No_Activities_Are_Confirmed()
        {
            IActivity activity = null;
            IActivity activity2 = null;

            using (_context)
            {
                activity = _context.Act(() => _moqObject.DoSomething(1)).CompensateWith(() => _moqObject.RevertSomething(1));
                activity.Execute();

                activity2 = _context.Act(() => _moqObject.DoSomething(2)).CancelWith(() => _moqObject.CancelSomething(2));
                activity2.Execute();

                _context.RollBack();
            }

            Assert.IsFalse(_context.Completed);
            Assert.IsTrue(_context.RolledBack);
            Assert.IsFalse(activity.Confirmed);
            Assert.IsFalse(activity2.Confirmed);
            _moq.Verify(m => m.DoSomething(1), Times.Once());
            _moq.Verify(m => m.DoSomething(2), Times.Once());
            _moq.Verify(m => m.RevertSomething(1), Times.Once());
            _moq.Verify(m => m.CancelSomething(2), Times.Never());
            _moq.Verify(m => m.DoSomething(3), Times.Never());
        }

        [TestMethod]
        public void When_Disposed_Then_Actions_Throw()
        {
            using (_context)
            {

            }

            var temp = new Action[]
            {
                () => _context.Act(() => ""),
                () => _context.Complete(),
                () => _context.RollBack(),
                () => _context.Execute(() => "", null, null)
            };

            foreach (var t in temp)
            {
                try
                {
                    t();
                }
                catch (ObjectDisposedException)
                {
                    continue;
                }
                catch (Exception e)
                {
                    Assert.Fail("Didn't throw proper Exception: {0} - {1}", e.GetType().Name, e.Message);
                }
                Assert.Fail("No exception thrown!");
            }
        }

        [TestMethod]
        public void When_rollbacking_Then_order_is_properly_reversed()
        {
            string value = "";
            _moq.Setup(x => x.RevertSomething(It.IsAny<int>())).Callback((int i) => value += i.ToString());
            //_moq.Setup(x => x.RevertSomething(2)).Callback(() => value = "2");
            try
            {
                using (_context)
                {
                    _context.Act(() => _moqObject.DoSomething(1)).CompensateWith(() => _moqObject.RevertSomething(1)).Execute();
                    _context.Act(() => _moqObject.DoSomething(2)).CompensateWith(() => _moqObject.RevertSomething(2)).Execute();

                    throw new ApplicationException(ExceptionMessage);
                }
            }
            catch (ApplicationException e)
            {
                Assert.AreEqual(ExceptionMessage, e.Message);
            }

            Assert.IsFalse(_context.Completed);
            Assert.IsTrue(_context.RolledBack);
            _moq.Verify(m => m.DoSomething(It.IsAny<int>()), Times.Exactly(2));
            _moq.Verify(m => m.RevertSomething(It.IsAny<int>()), Times.Exactly(2));
            Assert.AreEqual<string>("21", value); // Should have reverted the call with 2 first!
        }

        [TestMethod]
        public void When_rollbacking_and_executed_in_weird_order_Then_order_is_properly_reversed()
        {
            string value = "";
            _moq.Setup(x => x.RevertSomething(It.IsAny<int>())).Callback((int i) => value += i.ToString());
            //_moq.Setup(x => x.RevertSomething(2)).Callback(() => value = "2");
            try
            {
                using (_context)
                {
                    var activity1 = _context.Act(() => _moqObject.DoSomething(1)).CompensateWith(() => _moqObject.RevertSomething(1));
                    var activity2 = _context.Act(() => _moqObject.DoSomething(2)).CompensateWith(() => _moqObject.RevertSomething(2));
                    activity2.Execute();
                    activity1.Execute();

                    throw new ApplicationException(ExceptionMessage);
                }
            }
            catch (ApplicationException e)
            {
                Assert.AreEqual(ExceptionMessage, e.Message);
            }

            Assert.IsFalse(_context.Completed);
            Assert.IsTrue(_context.RolledBack);
            _moq.Verify(m => m.DoSomething(It.IsAny<int>()), Times.Exactly(2));
            _moq.Verify(m => m.RevertSomething(It.IsAny<int>()), Times.Exactly(2));
            Assert.AreEqual<string>("12", value, "Should have reverted the call with 1 first, because it was executed after!");
        }
    }
}
