See license.txt

Website: http://code.google.com/p/system-transactions/

Basic example:

using (var context = new WorkflowContext())
{
    context.Act(() => SomeMethodCall())
      .CompensateWith(() => SomeOtherMethodCall())
      .CancelWith(() => SomeOtherMethodCallAgain())
      .Execute();

    AThirdMethodCall();

    context.Complete();
}