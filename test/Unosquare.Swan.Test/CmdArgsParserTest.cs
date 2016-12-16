using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.Swan.Runtime;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class CmdArgsParserTest
    {
        [Test]
        public void BasicArgsTest()
        {
            var options = new OptionMock();
            Assert.IsFalse(options.Verbose);

            var dumpArgs = new[] { "-n", "babu", "--verbose" };
            var result = CmdArgsParser.Default.ParseArguments(dumpArgs, options);

            Assert.IsTrue(result);
            Assert.IsTrue(options.Verbose);
            Assert.AreEqual(dumpArgs[1], options.Username);
        }

        [Test]
        public void EnumArgTest()
        {
            var options = new OptionMock();
            Assert.AreEqual(ConsoleColor.Red, options.BgColor);

            var newColor = ConsoleColor.White;

            var dumpArgs = new[] { "-c", newColor.ToString().ToLowerInvariant() };
            var result = CmdArgsParser.Default.ParseArguments(dumpArgs, options);

            Assert.IsTrue(result);
            Assert.AreEqual(newColor, options.BgColor);
        }

        [Test]
        public void ListArgTest()
        {
            var options = new OptionMock();
            Assert.IsNull(options.Options);
            var collection = new[] { "ok","xor","zzz" };

            var dumpArgs = new[] { "--options", string.Join(",", collection) };
            var result = CmdArgsParser.Default.ParseArguments(dumpArgs, options);

            Assert.IsTrue(result);
            Assert.IsNotNull(options.Options);
            Assert.IsTrue(options.Options.Any());

            Assert.AreEqual(collection.Length, options.Options.Length);
            Assert.AreEqual(collection.First(), options.Options.First());
            Assert.AreEqual(collection.Last(), options.Options.Last());
        }
    }
}
