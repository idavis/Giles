using System;
using System.Reflection;
using Giles.Core.Runners;
using Machine.Specifications.Runner;
using Machine.Specifications.Runner.Impl;

namespace Giles.Runner.Machine.Specifications
{
    [Serializable]
    public class SpecificationRunner : MarshalByRefObject, IFrameworkRunner
    {
        public Assembly TestAssembly { get; set; }
        public AppDomain AppDomain { get; set; }
        AppDomain currentDomain;

        public SpecificationRunner()
        {
            currentDomain = AppDomain.CurrentDomain;
        }

        public AppDomain TestRunnerAppDomain
        {
            get { return AppDomain.CurrentDomain; }
        }

        public TestRunState RunAssembly(ITestListener testListener)
        {
            var listener = new GilesRunListener(testListener);
            var runner = new AppDomainRunner(listener, RunOptions.Default);
            runner.RunAssembly(TestAssembly);
            return listener.TestRunState;
        }
    }
}