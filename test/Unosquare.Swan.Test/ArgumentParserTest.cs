namespace Unosquare.Swan.Test.ArgumentParserTests
{
    using NUnit.Framework;
    using System;
    using System.Linq;
    using Components;
    using Mocks;

    [TestFixture]
    public class ParseArguments : TestFixtureBase
    {
        [Test]
        public void BasicArguments_ReturnsEquals()
        {
            var dumpArgs = new[] { "-n", "babu", "--verbose" };
            var result = Runtime.ArgumentParser.ParseArguments<OptionMock>(dumpArgs, out var options);

            Assert.IsTrue(result);
            Assert.IsTrue(options.Verbose);
            Assert.AreEqual(dumpArgs[1], options.Username);
            Assert.AreEqual(ConsoleColor.Red, options.BgColor, "Default color");
        }

        [Test]
        public void InvalidDataConversion_ReturnsFalse()
        {
            var options = new OptionIntRequiredMock();
            var result = Runtime.ArgumentParser.ParseArguments(new[] { "-n", "babu" }, options);

            Assert.IsFalse(result);
        }

        [Test]
        public void InvalidClassWithCollection_ReturnsFalse()
        {
            var options = new OptionObjectCollectionMock();
            var result = Runtime.ArgumentParser.ParseArguments(new[] { "--options", "1", "1", "0" }, options);

            Assert.IsFalse(result);
        }

        [Test]
        public void ValidObjectArray_ReturnsTrue()
        {
            var options = new OptionObjectArrayMock();
            var result = Runtime.ArgumentParser.ParseArguments(new[] { "--options", "1,null,0" }, options);

            Assert.IsTrue(result);
            Assert.AreEqual(3, options.Options.Length);
        }

        [Test]
        public void CaseSensitiveArguments_ReturnsFalse()
        {
            var options = new OptionMock();
            var dumpArgs = new[] { "-N", "babu", "-V" };
            var parser = new ArgumentParser(new ArgumentParserSettings { CaseSensitive = true });
            var result = parser.ParseArguments(dumpArgs, options);

            Assert.IsFalse(result, "Parsing is not valid");
        }

        [Test]
        public void UnknwownArguments_ReturnsFalse()
        {
            var options = new OptionMock();
            var dumpArgs = new[] { "-XOR" };
            var parser = new ArgumentParser(new ArgumentParserSettings { IgnoreUnknownArguments = false });
            var result = parser.ParseArguments(dumpArgs, options);

            Assert.IsFalse(result, "Argument is unknown");
        }

        [Test]
        public void EnumArguments_ReturnsTrue()
        {
            var options = new OptionMock();
            Assert.AreEqual(ConsoleColor.Black, options.BgColor);

            const ConsoleColor newColor = ConsoleColor.White;

            var dumpArgs = new[] { "-n", "babu", "--color", newColor.ToString().ToLowerInvariant() };
            var result = Runtime.ArgumentParser.ParseArguments(dumpArgs, options);

            Assert.IsTrue(result);
            Assert.AreEqual(newColor, options.BgColor);
        }

        [Test]
        public void ListArguments_ReturnsTrue()
        {
            var options = new OptionMock();
            Assert.IsNull(options.Options);

            var dumpArgs = new[] { "-n", "babu", "--options", string.Join(",", DefaultStringList) };
            var result = Runtime.ArgumentParser.ParseArguments(dumpArgs, options);

            Assert.IsTrue(result);
            Assert.IsNotNull(options.Options);
            Assert.IsTrue(options.Options.Any());

            Assert.AreEqual(DefaultStringList.Count, options.Options.Length);
            Assert.AreEqual(DefaultStringList.First(), options.Options.First());
            Assert.AreEqual(DefaultStringList.Last(), options.Options.Last());
        }

        [Test]
        public void UnavailableArguments_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Runtime.ArgumentParser.ParseArguments(null, new OptionMock()));
        }

        [Test]
        public void TypeInvalid_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                Runtime.ArgumentParser.ParseArguments(DefaultStringList, 1));
        }

        [Test]
        public void PropertiesEmpty_ThrowsInvalidOperationException()
        {
            var dumpArgs = new[] { "--options", string.Join(",", DefaultStringList) };

            Assert.Throws<InvalidOperationException>(() =>
                Runtime.ArgumentParser.ParseArguments(dumpArgs, new OptionMockEmpty()));
        }

        [Test]
        public void InstanceNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Runtime.ArgumentParser.ParseArguments<OptionMock>(DefaultStringList, null));
        }
    }

    [TestFixture]
    public class ParseVerbs : TestFixtureBase
    {
        [Test]
        public void EmptyArray_ReturnsFalse()
        {
            var verbOptions = new CliVerbs();
            var arguments = new string[0];
            var expected = Runtime.ArgumentParser.ParseArguments(arguments, verbOptions);

            Assert.IsFalse(expected);
        }

        [Test]
        public void BasicVerbParsing_ReturnsTrue()
        {
            var verbOptions = new CliVerbs();
            var arguments = new[] { "monitor", "-v" };
            var expected = Runtime.ArgumentParser.ParseArguments(arguments, verbOptions);

            Assert.IsTrue(expected);
        }

        [Test]
        public void BasicVerbParsing_InstantiatesSelectedVerbOptionProperty()
        {
            var verbOptions = new CliVerbs();
            var arguments = new[] { "monitor", "-v" };
            var expected = Runtime.ArgumentParser.ParseArguments(arguments, verbOptions);

            Assert.IsTrue(expected);
            Assert.IsNotNull(verbOptions.MonitorVerboptions);
            Assert.IsNull(verbOptions.PushVerbOptions);
        }

        [Test]
        public void NoValidVerbOptionSelected_ReturnsFalse()
        {
            var verbOptions = new CliVerbs();
            var arguments = new[] { "option", "-v" };
            var expected = Runtime.ArgumentParser.ParseArguments(arguments, verbOptions);

            Assert.IsFalse(expected);
            Assert.IsNull(verbOptions.MonitorVerboptions);
            Assert.IsNull(verbOptions.PushVerbOptions);
        }
    }
}