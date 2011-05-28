using System;
using System.Collections.Generic;
using Giles.Core.AppDomains;
using Giles.Core.Runners;
using Machine.Specifications;

namespace Giles.Specs.Core.AppDomains
{
    public class with_a_giles_app_domain_manager
    {
        protected static string path;
        protected static GilesAppDomainManager manager;
        protected static IEnumerable<SessionResults> result;

        Establish context = () =>
            manager = new GilesAppDomainManager();
    }

    public class when_running_tests_in_an_app_domain_with_a_null_path : with_a_giles_app_domain_manager
    {
        static Exception ex;

        Establish context = () =>
            path = null;

        Because of = () =>
            ex = Catch.Exception(() => result = manager.Run(path));

        It should_throw_an_argument_message = () =>
            ex.ShouldBeOfType<ArgumentNullException>();

        It should_tell_us_that_the_test_assembly_path_being_null_is_the_problem = () =>
            ex.Message.ShouldContain("Test Assembly Path cannot be null. Check your Giles configuration.");
    }
 }