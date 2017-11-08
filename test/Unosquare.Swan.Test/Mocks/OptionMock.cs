namespace Unosquare.Swan.Test.Mocks
{
    using System;
    using Attributes;
    using System.Collections.Generic;

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

    public class OptionIntRequiredMock
    {
        [ArgumentOption('n', Required = true, HelpText = "Set int.")]
        public int? IntValue { get; set; }
    }

    public class OptionObjectCollectionMock
    {
        [ArgumentOption('o', "options", Separator = ',', HelpText = "Specify additional options.")]
        public List<int?> Options { get; set; }
    }

    public class OptionObjectArrayMock
    {
        [ArgumentOption('o', "options", Separator = ',', HelpText = "Specify additional options.")]
        public int?[] Options { get; set; }
    }

    public class OptionMockEmpty
    {
        // empty
    }
}
