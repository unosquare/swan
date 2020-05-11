using Swan.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Swan.Parsers
{
    /// <summary>
    /// Provides methods to parse command line arguments.
    /// Based on CommandLine (Copyright 2005-2015 Giacomo Stelluti Scala and Contributors.).
    /// </summary>
    /// <example>
    /// The following example shows how to parse CLI arguments into objects.
    /// <code>
    /// class Example 
    /// {
    ///     using System;
    ///     using Swan.Parsers;
    ///     
    ///     static void Main(string[] args)
    ///     {
    ///         // parse the supplied command-line arguments into the options object
    ///         var res = Runtime.ArgumentParser.ParseArguments(args, out var options);
    ///     }
    ///     
    ///     class Options
    ///     {
    ///         [ArgumentOption('v', "verbose", HelpText = "Set verbose mode.")]
    ///         public bool Verbose { get; set; }
    ///
    ///         [ArgumentOption('u', Required = true, HelpText = "Set user name.")]
    ///         public string Username { get; set; }
    ///
    ///         [ArgumentOption('n', "names", Separator = ',',
    ///         Required = true, HelpText = "A list of files separated by a comma")]
    ///         public string[] Files { get; set; }
    ///         
    ///         [ArgumentOption('p', "port", DefaultValue = 22, HelpText = "Set port.")]
    ///         public int Port { get; set; }
    ///
    ///         [ArgumentOption("color", DefaultValue = ConsoleColor.Red,
    ///         HelpText = "Set a color.")]
    ///         public ConsoleColor Color { get; set; }
    ///     }
    /// }
    /// </code>
    /// The following code describes how to parse CLI verbs.
    /// <code>
    /// class Example2 
    /// {
    ///     using Swan;
    ///     using Swan.Parsers;
    ///     
    ///     static void Main(string[] args)
    ///     {
    ///         // create an instance of the VerbOptions class
    ///         var options = new VerbOptions();
    ///         
    ///         // parse the supplied command-line arguments into the options object
    ///         var res = Runtime.ArgumentParser.ParseArguments(args, options);
    ///         
    ///         // if there were no errors parsing
    ///         if (res)
    ///         {
    ///             if(options.Run != null)
    ///             {
    ///                 // run verb was selected
    ///             }
    ///             
    ///             if(options.Print != null)
    ///             {
    ///                 // print verb was selected
    ///             }
    ///         }
    ///         
    ///         // flush all error messages
    ///         Terminal.Flush();
    ///     }
    ///     
    ///     class VerbOptions
    ///     {
    ///         [VerbOption("run", HelpText = "Run verb.")]
    ///         public RunVerbOption Run { get; set; }
    ///         
    ///         [VerbOption("print", HelpText = "Print verb.")]
    ///         public PrintVerbOption Print { get; set; }
    ///     }
    ///     
    ///     class RunVerbOption
    ///     {
    ///         [ArgumentOption('o', "outdir", HelpText = "Output directory",
    ///         DefaultValue = "", Required = false)]
    ///         public string OutDir { get; set; }
    ///     }
    ///     
    ///     class PrintVerbOption
    ///     {
    ///         [ArgumentOption('t', "text", HelpText = "Text to print",
    ///         DefaultValue = "", Required = false)]
    ///         public string Text { get; set; }
    ///     }
    /// }
    /// </code>
    /// </example>
    public partial class ArgumentParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentParser"/> class.
        /// </summary>
        public ArgumentParser()
            : this(new ArgumentParserSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentParser" /> class,
        /// configurable with <see cref="ArgumentParserSettings" /> using a delegate.
        /// </summary>
        /// <param name="parseSettings">The parse settings.</param>
        public ArgumentParser(ArgumentParserSettings parseSettings)
        {
            Settings = parseSettings ?? throw new ArgumentNullException(nameof(parseSettings));
        }

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        public static ArgumentParser Current { get; } = new ArgumentParser();

        /// <summary>
        /// Gets the instance that implements <see cref="ArgumentParserSettings" /> in use.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public ArgumentParserSettings Settings { get; }
        
        /// <summary>
        /// Parses a string array of command line arguments constructing values in an instance of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of the options.</typeparam>
        /// <param name="args">The arguments.</param>
        /// <param name="instance">The instance.</param>
        /// <returns>
        /// <c>true</c> if was converted successfully; otherwise,  <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when a null reference (Nothing in Visual Basic) 
        /// is passed to a method that does not accept it as a valid argument.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The exception that is thrown when a method call is invalid for the object's current state.
        /// </exception>
        public bool ParseArguments<T>(IEnumerable<string> args, out T instance)
        {
            instance = Activator.CreateInstance<T>();
            return ParseArguments(args, instance);
        }

        /// <summary>
        /// Parses a string array of command line arguments constructing values in an instance of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of the options.</typeparam>
        /// <param name="args">The arguments.</param>
        /// <param name="instance">The instance.</param>
        /// <returns>
        /// <c>true</c> if was converted successfully; otherwise,  <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when a null reference (Nothing in Visual Basic) 
        /// is passed to a method that does not accept it as a valid argument.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The exception that is thrown when a method call is invalid for the object's current state.
        /// </exception>
        public bool ParseArguments<T>(IEnumerable<string> args, T instance)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (Equals(instance, default(T)))
                throw new ArgumentNullException(nameof(instance));

            var typeResolver = new TypeResolver<T>(args.FirstOrDefault());
            var options = typeResolver.GetOptionsObject(instance);

            if (options == null)
            {
                ReportUnknownVerb<T>();
                return false;
            }

            if (typeResolver.Properties == null)
                throw new InvalidOperationException($"Type {typeof(T).Name} is not valid");

            var validator = new Validator(typeResolver.Properties, args, options, Settings, typeResolver.HasVerb);

            if (validator.IsValid())
                return true;

            ReportIssues(validator);
            return false;
        }
        
        private static void ReportUnknownVerb<T>()
        {
            Terminal.WriteLine("No verb was specified", ConsoleColor.Red);
            Terminal.WriteLine("Valid verbs:", ConsoleColor.Cyan);

            PropertyTypeCache.DefaultCache.Value
                .RetrieveAllProperties<T>(true)
                .Select(x => AttributeCache.DefaultCache.Value.RetrieveOne<VerbOptionAttribute>(x))
                .Where(x => x != null)
                .ToList()
                .ForEach(x => Terminal.WriteLine(x.ToString(), ConsoleColor.Cyan));
        }

        private void ReportIssues(Validator validator)
        {
            if (Settings.WriteBanner)
                Terminal.WriteWelcomeBanner();

            var options = validator.GetPropertiesOptions();

            foreach (var option in options)
            {
                Terminal.WriteLine(string.Empty);

                // TODO: If Enum list values
                var shortName = string.IsNullOrWhiteSpace(option.ShortName) ? string.Empty : $"-{option.ShortName}";
                var longName = string.IsNullOrWhiteSpace(option.LongName) ? string.Empty : $"--{option.LongName}";
                var comma = string.IsNullOrWhiteSpace(shortName) || string.IsNullOrWhiteSpace(longName)
                    ? string.Empty
                    : ", ";
                var defaultValue = option.DefaultValue == null ? string.Empty : $"(Default: {option.DefaultValue}) ";

                Terminal.WriteLine($"  {shortName}{comma}{longName}\t\t{defaultValue}{option.HelpText}", ConsoleColor.Cyan);
            }

            Terminal.WriteLine(string.Empty);
            Terminal.WriteLine("  --help\t\tDisplay this help screen.", ConsoleColor.Cyan);

            if (validator.UnknownList.Any())
                Terminal.WriteLine($"Unknown arguments: {string.Join(", ", validator.UnknownList)}", ConsoleColor.Red);

            if (validator.RequiredList.Any())
                Terminal.WriteLine($"Required arguments: {string.Join(", ", validator.RequiredList)}", ConsoleColor.Red);
        }
    }
}
