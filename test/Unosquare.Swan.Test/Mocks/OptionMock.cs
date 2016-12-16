using System;
using Unosquare.Swan.Runtime;

namespace Unosquare.Swan.Test.Mocks
{
    public class OptionMock
    {
        [Option('v', "verbose", HelpText = "Set verbose mode.")]
        public bool Verbose { get; set; }

        [Option("color", DefaultValue = ConsoleColor.Red, HelpText = "Set background color.")]
        public ConsoleColor BgColor { get; set; }

        [Option('n', Required = true, HelpText = "Set user name.")]
        public string Username { get; set; }

        [Option('o', "options", Separator = ',', HelpText = "Specify additional options.")]
        public string[] Options { get; set; }
    }
}
