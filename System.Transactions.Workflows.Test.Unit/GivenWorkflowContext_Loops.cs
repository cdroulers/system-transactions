using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace System.Transactions.Workflows.Test.Unit
{
    [TestClass]
    public class GivenWorkflowContext_Loops
    {
        private Mock<ITestRepo<int>> _moq;
        private ITestRepo<int> _moqObject;
        private WorkflowContext _context;
        private int[] ForEachListValueTypes;
        private TempClass[] ForEachListReferenceTypes;
        private Mock<ITestRepo<TempClass>> _moqReference;
        private ITestRepo<TempClass> _moqReferenceObject;

        public class TempClass
        {
            public TempClass(string data)
            {
                this.Data = data;
            }

            public string Data { get; set; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(TempClass)) return false;
                return Equals((TempClass)obj);
            }

            public override int GetHashCode()
            {
                return (this.Data ?? string.Empty).GetHashCode();
            }

            public bool Equals(TempClass other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other.Data, this.Data);
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _moq = new Mock<ITestRepo<int>>();
            _moqObject = _moq.Object;
            _context = new WorkflowContext();
            ForEachListValueTypes = new int[] { 1, 2, 3 };

            _moqReference = new Mock<ITestRepo<TempClass>>();
            _moqReferenceObject = _moqReference.Object;
            ForEachListReferenceTypes = new TempClass[] { new TempClass("1"), new TempClass("2"), new TempClass("3") };
        }

        [TestMethod]
        public void When_ForEach_Looping_Then_All_Is_Well()
        {
            using (_context)
            {
                foreach (var item in ForEachListValueTypes)
                {
                    _context.Act(() => this._moqObject.DoSomething(item)).CompensateWith(() => this._moqObject.RevertSomething(item)).Execute();
                }

                _context.Complete();
            }

            Assert.IsTrue(_context.Completed);
            Assert.IsFalse(_context.RolledBack);
            _moq.Verify(m => m.DoSomething(1), Times.Once());
            _moq.Verify(m => m.DoSomething(2), Times.Once());
            _moq.Verify(m => m.DoSomething(3), Times.Once());
            _moq.Verify(m => m.RevertSomething(1), Times.Never());
            _moq.Verify(m => m.RevertSomething(2), Times.Never());
            _moq.Verify(m => m.RevertSomething(3), Times.Never());
        }

        [TestMethod]
        public void When_ForEach_Looping_Value_Types_And_RollBacking_Then_RollBack_Does_Not_Execute_With_Proper_Parameter()
        {
            using (_context)
            {
                foreach (var item in ForEachListValueTypes)
                {
                    _context.Act(() => this._moqObject.DoSomething(item)).CompensateWith(() => this._moqObject.RevertSomething(item)).Execute();
                }

                _context.RollBack();
            }

            Assert.IsFalse(_context.Completed);
            Assert.IsTrue(_context.RolledBack);
            _moq.Verify(m => m.DoSomething(1), Times.Once());
            _moq.Verify(m => m.DoSomething(2), Times.Once());
            _moq.Verify(m => m.DoSomething(3), Times.Once());
            _moq.Verify(m => m.RevertSomething(1), Times.Never());
            _moq.Verify(m => m.RevertSomething(2), Times.Never());
            _moq.Verify(m => m.RevertSomething(3), Times.Exactly(3));
        }

        [TestMethod]
        public void When_ForEach_Looping_Reference_Types_And_RollBacking_Then_RollBack_Does_Not_Execute_With_Proper_Parameter()
        {
            using (_context)
            {
                foreach (var item in ForEachListReferenceTypes)
                {
                    _context.Act(() =>
                        this._moqReferenceObject.DoSomething(item)
                    ).CompensateWith(() =>
                        this._moqReferenceObject.RevertSomething(item)
                    ).Execute();
                }

                _context.RollBack();
            }

            Assert.IsFalse(_context.Completed);
            Assert.IsTrue(_context.RolledBack);
            _moqReference.Verify(m => m.DoSomething(new TempClass("1")), Times.Once());
            _moqReference.Verify(m => m.DoSomething(new TempClass("2")), Times.Once());
            _moqReference.Verify(m => m.DoSomething(new TempClass("3")), Times.Once());
            _moqReference.Verify(m => m.RevertSomething(new TempClass("1")), Times.Never());
            _moqReference.Verify(m => m.RevertSomething(new TempClass("2")), Times.Never());
            _moqReference.Verify(m => m.RevertSomething(new TempClass("3")), Times.Exactly(3));
        }

        [TestMethod]
        public void When_ForEach_Looping_Reference_Types_And_RollBacking_Then_RollBack_Does_Not_Execute_With_Proper_Parameter_Workaround()
        {
            using (_context)
            {
                foreach (var item in ForEachListReferenceTypes)
                {
                    var temp = item;
                    _context.Act(() =>
                        this._moqReferenceObject.DoSomething(temp)
                    ).CompensateWith(() =>
                        this._moqReferenceObject.RevertSomething(temp)
                    ).Execute();
                }

                _context.RollBack();
            }

            Assert.IsFalse(_context.Completed);
            Assert.IsTrue(_context.RolledBack);
            _moqReference.Verify(m => m.DoSomething(new TempClass("1")), Times.Once());
            _moqReference.Verify(m => m.DoSomething(new TempClass("2")), Times.Once());
            _moqReference.Verify(m => m.DoSomething(new TempClass("3")), Times.Once());
            _moqReference.Verify(m => m.RevertSomething(new TempClass("1")), Times.Once());
            _moqReference.Verify(m => m.RevertSomething(new TempClass("2")), Times.Once());
            _moqReference.Verify(m => m.RevertSomething(new TempClass("3")), Times.Once());
        }
    }
}
