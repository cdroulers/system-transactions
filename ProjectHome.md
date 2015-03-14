A very basic framework to handle transactions in an environment where they aren't available (such as a multiple back-end manager).

It would be used to declare code lines that can be rolled back if a transaction fails. The goal is to take out the basics of the [CompensableActivity](http://msdn.microsoft.com/en-us/library/system.activities.statements.compensableactivity.aspx) from Windows Workflow Foundation and make it more fluent and easier to debug.

```
using (var context = new WorkflowContext())
{
    context.Act(() => SomeMethodCall())
      .CompensateWith(() => SomeOtherMethodCall())
      .CancelWith(() => SomeOtherMethodCallAgain())
      .Execute();

    AThirdMethodCall();

    context.Complete();
}
```

In the previous code, if _SomeMethodCall()_ throws an exception, _SomeOtherMethodCallAgain()_ will be executed, and the exception will be re-thrown, as expected. If _AThirdMethodCall()_ fails, _SomeOtherMethodCall()_ will be executed, and the exception will be re-thrown.

It makes it easy to build workflows that affect multiple back-ends (e.g. Active Directory, Database, Web Service) and make sure that method calls can be "atomic".

# Downloads #
NuGet package: https://nuget.org/packages/System.Transactions.Workflows/