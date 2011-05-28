using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Machine.Specifications;

namespace Giles.Specs.Console
{
    public class with_a_giles_console_application
    {
        protected static Process proc;
        protected static readonly List<Tuple<object, DataReceivedEventArgs>> Errors = new List<Tuple<object, DataReceivedEventArgs>>();
        protected static readonly List<Tuple<object, DataReceivedEventArgs>> Output = new List<Tuple<object, DataReceivedEventArgs>>();
        protected static ProcessStartInfo startInfo;
        protected static string Arguments;

        protected static string GetGilesConsoleAppLocation()
        {
            return typeof(SlayerModule).Assembly.Location;
        }

        Establish context = () =>
            {
                proc = new Process { EnableRaisingEvents = true };
                proc.ErrorDataReceived += (o, args) => Errors.Add(Tuple.Create(o, args));
                proc.OutputDataReceived += (o, args) => Output.Add(Tuple.Create(o, args));
            };

        Cleanup please = () =>
            {
                proc.Close();
                proc.Dispose();
            };

        protected static void SetupStartInfo()
        {
            startInfo = new ProcessStartInfo(GetGilesConsoleAppLocation(), Arguments)
                            {
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true
                            };
            proc.StartInfo = startInfo;
        }
    }

    public class when_starting_giles_without_command_line_parameters : with_a_giles_console_application
    {
        Establish context = () =>
            {
                Arguments = string.Empty;
                SetupStartInfo();
            };

        Because of = () =>
            {
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
            };

        It should_write_out_the_Giles_options_to_the_console = () => 
            Errors.Any(x => x.Item2.Data == "Giles Options").ShouldBeTrue();

        It should_write_out_information_to_the_console = () =>
            Output.Any(x => x.Item2.Data == "Unknown command line arguments").ShouldBeTrue();

        It should_have_an_exit_code_of_1 = () =>
            proc.ExitCode.ShouldEqual(1);
    }
}