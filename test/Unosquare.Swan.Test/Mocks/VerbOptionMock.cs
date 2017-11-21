using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unosquare.Swan.Attributes;

namespace Unosquare.Swan.Test.Mocks
{
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
