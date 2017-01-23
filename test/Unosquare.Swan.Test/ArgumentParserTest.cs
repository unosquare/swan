using NUnit.Framework;
using System;
using System.Linq;
using Unosquare.Swan.Components;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ArgumentParserTest
    {
        [Test]
        public void BasicArgsTest()
        {
            var options = new OptionMock();
            Assert.IsFalse(options.Verbose);

            var dumpArgs = new[] { "-n", "babu", "--verbose" };
            var result = Runtime.ArgumentParser.ParseArguments(dumpArgs, options);

            Assert.IsTrue(result);
            Assert.IsTrue(options.Verbose);
            Assert.AreEqual(dumpArgs[1], options.Username);
            Assert.AreEqual(ConsoleColor.Red, options.BgColor, "Default color");
        }

        [Test]
        public void CaseSensitiveArgsTest()
        {
            var options = new OptionMock();
            var dumpArgs = new[] { "-N", "babu", "-V" };
            var parser = new ArgumentParser(new ArgumentParserSettings { CaseSensitive = true });
            var result = parser.ParseArguments(dumpArgs, options);

            Assert.IsFalse(result, "Parsing is not valid");
        }

        [Test]
        public void UnknwownArgsTest()
        {
            var options = new OptionMock();
            var dumpArgs = new[] { "-XOR" };
            var parser = new ArgumentParser(new ArgumentParserSettings { IgnoreUnknownArguments = false });
            var result = parser.ParseArguments(dumpArgs, options);

            Assert.IsFalse(result, "Argument is unknown");
        }

        [Test]
        public void EnumArgTest()
        {
            var options = new OptionMock();
            Assert.AreEqual(ConsoleColor.Black, options.BgColor);

            var newColor = ConsoleColor.White;

            var dumpArgs = new[] { "-n", "babu", "--color", newColor.ToString().ToLowerInvariant() };
            var result = Runtime.ArgumentParser.ParseArguments(dumpArgs, options);

            Assert.IsTrue(result);
            Assert.AreEqual(newColor, options.BgColor);
        }

        [Test]
        public void ListArgTest()
        {
            var options = new OptionMock();
            Assert.IsNull(options.Options);
            var collection = new[] { "ok","xor","zzz" };

            var dumpArgs = new[] { "-n", "babu", "--options", string.Join(",", collection) };
            var result = Runtime.ArgumentParser.ParseArguments(dumpArgs, options);

            Assert.IsTrue(result);
            Assert.IsNotNull(options.Options);
            Assert.IsTrue(options.Options.Any());

            Assert.AreEqual(collection.Length, options.Options.Length);
            Assert.AreEqual(collection.First(), options.Options.First());
            Assert.AreEqual(collection.Last(), options.Options.Last());
        }

        [Test]
        public void ThrowErrorTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var parser = new ArgumentParser();
                var options = new OptionMock();
                parser.ParseArguments(null, options);
            });
        }
    }
}
