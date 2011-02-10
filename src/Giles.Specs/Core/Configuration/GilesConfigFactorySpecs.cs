using System.IO;
using System.Linq;
using Giles.Core.Configuration;
using Giles.Core.IO;
using Machine.Specifications;
using Machine.Specifications.Utility;
using NSubstitute;

namespace Giles.Specs.Core.Configuration
{
    public class a_giles_config
    {
        protected static GilesConfigFactory factory;
        protected static GilesConfig config;
        protected static IFileSystem fileSystem;
        protected static string solutionPath;
        protected static string solutionFolder;
        protected static string testRunnerExe;
        protected static string testAssemblyPath;

        Establish context = () =>
                                {
                                    solutionFolder = @"c:\solutionFolder";
                                    solutionPath = @"c:\solutionFolder\mySolution.sln";
                                    testAssemblyPath = @"c:\solutionFolder\tests\bin\debug\tests.dll";
                                    fileSystem = Substitute.For<IFileSystem>();
                                    testRunnerExe = @"c:\testAssembly.exe";
                                    fileSystem.GetFiles(Arg.Any<string>(), Arg.Any<string>(), SearchOption.AllDirectories)
                                        .Returns(new[] { testRunnerExe });
                                    fileSystem.GetDirectoryName(solutionPath).Returns(solutionFolder);

                                    config = new GilesConfig();
                                    factory = new GilesConfigFactory(config, fileSystem, solutionPath, testAssemblyPath);
                                };

    }

    public class when_building : a_giles_config
    {
        Because of = () =>
            factory.Build();

        It gets_the_solution_folder = () =>
            fileSystem.Received().GetDirectoryName(solutionPath);

        It locates_the_mspec_test_runner = () =>
            fileSystem.Received().GetFiles(solutionFolder, "mspec.exe", SearchOption.AllDirectories);

        It locates_the_nunit_test_runner = () =>
            fileSystem.Received().GetFiles(solutionFolder, "nunit-console.exe", SearchOption.AllDirectories);

        It built_the_correct_config_test_runners = () =>
            config.TestRunners.All(x => x.Value.Path == testRunnerExe).ShouldBeTrue();

        It assigned_the_test_assembly_path = () =>
            config.TestAssemblyPath.ShouldEqual(testAssemblyPath);

        It assigned_the_solution_path = () =>
            config.SolutionPath.ShouldEqual(solutionPath);
			
    }

}