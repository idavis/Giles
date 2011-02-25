using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Giles.Core.Configuration;

namespace Giles.Core.Runners
{
    public interface ITestRunnerResolver
    {
        IEnumerable<IFrameworkRunner> Resolve(string assemblyLocation);
    }


    public class TestRunnerResolver : ITestRunnerResolver
    {


        public IEnumerable<IFrameworkRunner> Resolve(string assemblyLocation)
        {
            AppDomain newGilesAppDomain = null;

            try
            {
                newGilesAppDomain = AppDomainHelper.CreateAppDomain(Assembly.GetAssembly(typeof(GilesAssemblyRunner)).Location);

                var assemblyRunner =
                    newGilesAppDomain.CreateInstanceFromAndUnwrap("Giles.Core.dll",
                                                                  typeof(GilesAssemblyRunner).FullName,
                                                                  true,
                                                                  0,
                                                                  null,
                                                                  null,
                                                                  null,
                                                                  null) as GilesAssemblyRunner;

                var result = assemblyRunner.LoadFrom(assemblyLocation);

                // NOTE: I think what is happening here is the result from assemblyRunner.LoadFrom,
                // which is an instance of SpecificationRunner, is getting loaded into the current
                // domain instead of staying as a reference in the testRunnerAppDomain created in GetMSPecRunner

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                AppDomain.Unload(newGilesAppDomain);
                newGilesAppDomain = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            return null;
        }
    }


    internal class TestFrameworkRunner
    {
        internal Func<AssemblyName, bool> CheckReference { get; set; }
        internal Func<string, IFrameworkRunner> GetTheRunner { get; set; }
    }



    public static class AppDomainHelper
    {
        public static AppDomain CreateAppDomain(string assemblyLocation)
        {
            var domainSetup = GetDomainSetup(assemblyLocation);

            return AppDomain.CreateDomain(domainSetup.ApplicationName, null, domainSetup);
        }

        static AppDomainSetup GetDomainSetup(string assemblyLocation)
        {
            var domainSetup = new AppDomainSetup
                {
                    ApplicationBase = Path.GetDirectoryName(assemblyLocation),
                    ApplicationName = new FileInfo(assemblyLocation).Name + DateTime.Now.Ticks,
                    ConfigurationFile = GetConfigFile(assemblyLocation),
                    ShadowCopyFiles = "true",
                    ShadowCopyDirectories = Path.GetDirectoryName(assemblyLocation)
                };
            domainSetup.CachePath = domainSetup.ApplicationName;
            return domainSetup;
        }

        static string GetConfigFile(string assemblyLocation)
        {
            var configFile = assemblyLocation + ".config";

            return File.Exists(configFile) ? configFile : null;
        }
    }






    internal class GilesAssemblyRunner : MarshalByRefObject
    {
        List<TestFrameworkRunner> runners;

        readonly Func<AssemblyName, bool> mSpecRunnerPredicate =
            assemblyName => assemblyName.Name == "Machine.Specifications";


        void BuildRunnerList()
        {
            runners = new List<TestFrameworkRunner>
                {new TestFrameworkRunner {CheckReference = mSpecRunnerPredicate, GetTheRunner = GetMSpecRunner}};
        }

        public GilesAssemblyRunner()
        {
            BuildRunnerList();
        }

        public List<IFrameworkRunner> LoadFrom(string assemblyLocation)
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyLocation);

                //var assemblyInfo = new AssemblyNameProxy().GetAssemblyName(assemblyLocation);

                //var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                //var assembly = assemblies.Where(x => x.FullName == assemblyInfo.FullName).FirstOrDefault();
                var referencedAssemblies =
                    assembly.
                        GetReferencedAssemblies();

                var testFrameworkRunners = runners.Where(runner => referencedAssemblies.Count(runner.CheckReference) > 0);

                var frameworkRunners = testFrameworkRunners.Select(
                    runner => runner.GetTheRunner.Invoke(assemblyLocation)).ToList();

                frameworkRunners.ForEach(x => x.TestAssembly = assembly);

                referencedAssemblies = null;
                testFrameworkRunners = null;

                return
                    frameworkRunners;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }


        static IFrameworkRunner GetMSpecRunner(string testAssemblyLocation)
        {
            var runnerAssemblyLocation =
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                             "Giles.Runner.Machine.Specifications.dll");
            var coreAssemblyLocation =
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                             "Giles.Core.dll");

            var runner = GetRunner(runnerAssemblyLocation);

            if (runner == null)
                return null;


            var testRunnerAppDomain = AppDomainHelper.CreateAppDomain(testAssemblyLocation);

            var settings = testRunnerAppDomain.CreateInstanceFromAndUnwrap(coreAssemblyLocation, typeof(Settings).FullName ) as Settings;
            var result = testRunnerAppDomain.CreateInstanceFromAndUnwrap(runnerAssemblyLocation, runner.FullName) as IFrameworkRunner;
            result.AppDomain = testRunnerAppDomain;

            return result;
        }


        static Type GetRunner(string assemblyLocation)
        {
            return Assembly.LoadFrom(assemblyLocation).GetTypes()
                .Where(x => typeof(IFrameworkRunner).IsAssignableFrom(x) && x.IsClass)
                .FirstOrDefault();
        }

    }
}