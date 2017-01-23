using System;

namespace Unosquare.Swan.Test.Mocks
{
    public class OptionMock
    {
        [ArgumentOption('v', "verbose", HelpText = "Set verbose mode.")]
        public bool Verbose { get; set; }

        [ArgumentOption("color", DefaultValue = ConsoleColor.Red, HelpText = "Set background color.")]
        public ConsoleColor BgColor { get; set; }

        [ArgumentOption('n', Required = true, HelpText = "Set user name.")]
        public string Username { get; set; }

        [ArgumentOption('o', "options", Separator = ',', HelpText = "Specify additional options.")]
        public string[] Options { get; set; }
    }
}
