using System;
using Unosquare.Swan.Runtime;

namespace Unosquare.Swan.Test.Mocks
{
    public class OptionMock
    {
        [Option('v', "verbose", HelpText = "Set verbose mode")]
        public bool Verbose { get; set; }

        [Option("color", Default = ConsoleColor.Red, HelpText = "Set background color")]
        public ConsoleColor BgColor { get; set; }

        [Option('n', Required = true, HelpText = "User name")]
        public string Username { get; set; }
    }
}
