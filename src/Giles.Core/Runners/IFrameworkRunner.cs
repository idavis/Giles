using System;
using System.Reflection;

namespace Giles.Core.Runners
{
    public interface IFrameworkRunner
    {
        TestRunState RunAssembly(TestListenerBase testListener);
        //TestRunState RunAssembly(ITestListener testListener, Assembly assembly);
        //TestRunState RunNamespace(ITestListener testListener, Assembly assembly, string ns);
        //TestRunState RunMember(ITestListener testListener, Assembly assembly, MemberInfo member);
        Assembly TestAssembly { get; set; }
        AppDomain AppDomain { get; set; }
    }
}