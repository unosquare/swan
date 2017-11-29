namespace Unosquare.Swan.Test.Mocks
{
    using System;
    using Attributes;

    public class CliVerbs
    {
        [VerbOption("verb")]
        public VerbOptions PushVerbOptions { get; set; }
        [VerbOption("monitor")]
        public MonitorOptions MonitorVerboptions { get; set; }
    }

    public class VerbOptions
    {
        [ArgumentOption('v', "verbose", HelpText = "Set verbose mode.")]
        public bool Verbose { get; set; }

        [ArgumentOption("color", DefaultValue = ConsoleColor.Red, HelpText = "Set background color.")]
        public ConsoleColor BgColor { get; set; }

        [ArgumentOption('n', HelpText = "Set user name.")]
        public string Username { get; set; }
    }

    public class MonitorOptions
    {
        [ArgumentOption('v', "verbose", HelpText = "Set verbose mode.")]
        public bool Verbose { get; set; }

        [ArgumentOption("color", DefaultValue = ConsoleColor.Red, HelpText = "Set background color.")]
        public ConsoleColor BgColor { get; set; }
    }
}
