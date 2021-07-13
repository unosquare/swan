using Swan.Parsers;
using System;

namespace Swan.Test.Mocks
{
    public class CliVerbs
    {
        [VerbOption("verb")]
        public VerbOptions PushVerbOptions { get; set; }
        [VerbOption("monitor")]
        public MonitorOptions MonitorVerbOptions { get; set; }
    }

    public class VerbOptions
    {
        [ArgumentOption('v', "verbose", DefaultValue = true, HelpText = "Add this option to print messages to standard error and standard output streams. 0 to disable, any other number to enable.", Required = false)]
        public bool Verbose { get; set; }

        [ArgumentOption('h', "host", DefaultValue = 1, HelpText = "Hostname or IP Address of the target. -- Must be running an SSH server.", Required = true)]
        public string Host { get; set; }

        [ArgumentOption('p', "port", DefaultValue = 22, HelpText = "Port on which SSH is running..")]
        public int Port { get; set; }

        [ArgumentOption('u', "username", DefaultValue = "pi", HelpText = "The username under which the connection will be established.")]
        public string Username { get; set; }

        [ArgumentOption('w', "password", DefaultValue = "raspberry", HelpText = "The password for the given username.", Required = false)]
        public string Password { get; set; }

        [ArgumentOption("pre", HelpText = "Command to execute prior file transfer to target", Required = false)]
        public string PreCommand { get; set; }

        [ArgumentOption("post", HelpText = "Command to execute after file transfer to target", Required = false)]
        public string PostCommand { get; set; }

        [ArgumentOption("clean", DefaultValue = false, HelpText = "Deletes all files and folders on the target before pushing the new files.  0 to disable, any other number to enable.", Required = false)]
        public bool CleanTarget { get; set; }

        [ArgumentOption("exclude", DefaultValue = ".ready|.vshost.exe|.vshost.exe.config", HelpText = "a pipe (|) separated list of file suffixes to ignore while deploying.", Required = false)]
        public string ExcludeFileSuffixes { get; set; }

        public string[] ExcludeFileSuffixList
        {
            get
            {
                var ignoreFileSuffixes = string.IsNullOrWhiteSpace(ExcludeFileSuffixes) ?
                    new string[] { } :
                    ExcludeFileSuffixes.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                return ignoreFileSuffixes;
            }
        }
    }

    public class MonitorOptions
    {
        [ArgumentOption('v', "verbose", HelpText = "Set verbose mode.")]
        public bool Verbose { get; set; }

        [ArgumentOption("color", DefaultValue = ConsoleColor.Red, HelpText = "Set background color.")]
        public ConsoleColor BgColor { get; set; }
    }
}