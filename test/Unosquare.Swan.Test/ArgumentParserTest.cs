namespace Unosquare.Swan.Test.ArgumentParserTests
{
    using NUnit.Framework;
    using System;
    using System.Linq;
    using Components;
    using Mocks;

    [TestFixture]
    public class ParseArguments
    {
        [Test]
        public void BasicArguments_ReturnsEquals()
        {
            var options = new OptionMock();
            Assert.IsFalse(options.Verbose);

            var dumpArgs = new[] {"-n", "babu", "--verbose"};
            var result = Runtime.ArgumentParser.ParseArguments(dumpArgs, options);

            Assert.IsTrue(result);
            Assert.IsTrue(options.Verbose);
            Assert.AreEqual(dumpArgs[1], options.Username);
            Assert.AreEqual(ConsoleColor.Red, options.BgColor, "Default color");
        }

        [Test]
        public void InvalidDataConversion_ReturnsFalse()
        {
            var options = new OptionIntRequiredMock();
            var result = Runtime.ArgumentParser.ParseArguments(new[] {"-n", "babu"}, options);

            Assert.IsFalse(result);
        }

        [Test]
        public void ObjectCollection_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var options = new OptionObjectCollectionMock();
                Runtime.ArgumentParser.ParseArguments(new[] {"--options", "1", null, "0"}, options);
            });
        }

        [Test]
        public void ObjectArray_ReturnsTrue()
        {
            var options = new OptionObjectArrayMock();
            var result = Runtime.ArgumentParser.ParseArguments(new[] {"--options", "1,null,0"}, options);

            Assert.IsTrue(result);
            Assert.AreEqual(3, options.Options.Length);
        }

        [Test]
        public void CaseSensitiveArguments_ReturnsFalse()
        {
            var options = new OptionMock();
            var dumpArgs = new[] {"-N", "babu", "-V"};
            var parser = new ArgumentParser(new ArgumentParserSettings {CaseSensitive = true});
            var result = parser.ParseArguments(dumpArgs, options);

            Assert.IsFalse(result, "Parsing is not valid");
        }

        [Test]
        public void UnknwownArguments_ReturnsFalse()
        {
            var options = new OptionMock();
            var dumpArgs = new[] {"-XOR"};
            var parser = new ArgumentParser(new ArgumentParserSettings {IgnoreUnknownArguments = false});
            var result = parser.ParseArguments(dumpArgs, options);

            Assert.IsFalse(result, "Argument is unknown");
        }

        [Test]
        public void EnumArguments_ReturnsTrue()
        {
            var options = new OptionMock();
            Assert.AreEqual(ConsoleColor.Black, options.BgColor);

            const ConsoleColor newColor = ConsoleColor.White;

            var dumpArgs = new[] {"-n", "babu", "--color", newColor.ToString().ToLowerInvariant()};
            var result = Runtime.ArgumentParser.ParseArguments(dumpArgs, options);

            Assert.IsTrue(result);
            Assert.AreEqual(newColor, options.BgColor);
        }

        [Test]
        public void ListArguments_ReturnsTrue()
        {
            var options = new OptionMock();
            Assert.IsNull(options.Options);
            var collection = new[] {"ok", "xor", "zzz"};

            var dumpArgs = new[] {"-n", "babu", "--options", string.Join(",", collection)};
            var result = Runtime.ArgumentParser.ParseArguments(dumpArgs, options);

            Assert.IsTrue(result);
            Assert.IsNotNull(options.Options);
            Assert.IsTrue(options.Options.Any());

            Assert.AreEqual(collection.Length, options.Options.Length);
            Assert.AreEqual(collection.First(), options.Options.First());
            Assert.AreEqual(collection.Last(), options.Options.Last());
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
                Runtime.ArgumentParser.ParseArguments(new[] {"Alejandro", "Mariana", "Federico", "Víctor"}, 1));
        }

        [Test]
        public void PropertiesEmpty_ThrowsInvalidOperationException()
        {
            var collection = new[] {"v", "n", "color"};
            var dumpArgs = new[] {"--options", string.Join(",", collection)};

            Assert.Throws<InvalidOperationException>(() =>
                Runtime.ArgumentParser.ParseArguments(dumpArgs, new OptionMockEmpty()));
        }

        [Test]
        public void InstanceNull_ThrowsArgumentNullException()
        {
            var dumpArgs = new[] {"-N", "babu", "-V"};

            Assert.Throws<ArgumentNullException>(() =>
            {
                Runtime.ArgumentParser.ParseArguments<OptionMock>(dumpArgs, null);
            });
        }
    }
}