using System;

namespace Giles.Core.Runners
{
    public abstract class TestListenerBase : MarshalByRefObject
    {
        public abstract void WriteLine(string text, string category);
        public abstract void AddTestSummary(TestResult summary);
    }
}