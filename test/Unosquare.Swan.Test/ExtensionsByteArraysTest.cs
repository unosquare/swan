using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test.ExtensionsByteArraysTest
{
    public abstract class ExtensionsByteArraysTest
    {
        protected const int Value = 123456789;
        protected readonly byte[] _bytes = BitConverter.GetBytes(Value);
        protected readonly byte[] _nullBytes = null;
    }

    [TestFixture]
    public class ToLowerHex : ExtensionsByteArraysTest
    {
        [TestCase("0x15cd5b07", true)]
        [TestCase("15cd5b07", false)]
        public void ToLowerHexTest(string expected, bool prefix)
        {
            Assert.AreEqual(expected, _bytes.ToLowerHex(prefix), "Get ToLowerHex value");
        }
    }

    [TestFixture]
    public class ToUpperHex : ExtensionsByteArraysTest
    {
        [TestCase("0x15CD5B07", true)]
        [TestCase("15CD5B07", false)]
        public void WithValidBytes_ReturnsString(string expected, bool prefix)
        {
            Assert.AreEqual(expected, _bytes.ToUpperHex(prefix), $"Get ToUpperHex value");
        }
    }

    [TestFixture]
    public class ToDashedHex : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsString()
        {
            Assert.AreEqual("15-CD-5B-07", _bytes.ToDashedHex(), $"Get ToDashedHex value");
        }
    }

    [TestFixture]
    public class ToBase64 : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsString()
        {
            Assert.AreEqual("Fc1bBw==", _bytes.ToBase64(), $"Get ToBase64 value");
        }
    }

    [TestFixture]
    public class ConvertHexadecimalToBytes : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidHex_ReturnsString()
        {
            const string hex = "15CD5B07";
            Assert.AreEqual(_bytes, hex.ConvertHexadecimalToBytes(), $"Get ConvertHexadecimalToBytes value");
        }
    }

    [TestFixture]
    public class GetBitValueAt : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsBitValue()
        {
            Assert.AreEqual(0, _bytes[0].GetBitValueAt(1), $"Get GetBitValueAt value");
        }
    }

    [TestFixture]
    public class Split : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsSplitedString()
        {
            var expected = new List<byte[]>() { new byte[] { 91, 7 } };
            var sequence = BitConverter.GetBytes(456);
            Assert.AreEqual(expected, _bytes.Split(2, sequence), $"Get Split value");
        }

        [Test]
        public void WithNullSequence_ThrowsArgumentNullException()
        {
            var expected = new List<byte[]>() { new byte[] { 91, 7 } };
            var sequence = BitConverter.GetBytes(456);

            Assert.Throws<ArgumentNullException>(() => 
                _nullBytes.Split(2, sequence)
            );
        }

        [Test]
        public void WithNullBytes_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
                _bytes.Split(2, null)
            );
        }
    }

    [TestFixture]
    public class GetIndexOf : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsIndexOf()
        {
            Assert.AreEqual(-1, _bytes.GetIndexOf(new byte[0]), "Get index of empty array is -1");
        }

        [Test]
        public void WithNullSequence_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
                _bytes.GetIndexOf(null)
            );
        }
    }

    [TestFixture]
    public class DeepClone : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsArray()
        {
            Assert.AreEqual(_bytes, _bytes.DeepClone(), "Get DeepClone value");
        }
    }

    [TestFixture]
    public class Trim : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsTrimValue()
        {
            Assert.AreEqual(new byte[] { 21, 205, 91, 7 }, _bytes.Trim(205), "Get Trim value");
        }
    }

    [TestFixture]
    public class TrimStart : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsTrimStartValue()
        {
            Assert.AreEqual(new byte[] { 205, 91, 7 }, _bytes.TrimStart(21), "Get TrimStart value");
        }
    }

    [TestFixture]
    public class TrimEnd : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsTrimEndValue()
        {
            Assert.AreEqual(new byte[] { 21, 205, 91 }, _bytes.TrimEnd(7), "Get TrimEnd value");
        }
    }

    [TestFixture]
    public class EndsWith : ExtensionsByteArraysTest
    {
        [TestCase(true, 7)]
        [TestCase(false, 21)]
        public void WithValidBytes_ReturnsEndsWithValue(bool expected, byte input)
        {
            Assert.AreEqual(expected, _bytes.EndsWith(input), "Get EndsWith value");
        }
    }

    [TestFixture]
    public class StartsWith : ExtensionsByteArraysTest
    {
        [TestCase(false, 7)]
        [TestCase(true, 21)]
        public void WithValidBytes_ReturnsStartsWithValue(bool expected, byte input)
        {
            Assert.AreEqual(expected, _bytes.StartsWith(input), "Get StartsWith value");
        }
    }

    [TestFixture]
    public class Contains : ExtensionsByteArraysTest
    {
        [TestCase(true, 91)]
        [TestCase(false, 92)]
        public void WithValidBytes_ReturnsContainsValue(bool expected, byte input)
        {
            Assert.AreEqual(expected, _bytes.Contains(input), "Get Contains value");
        }
    }

    [TestFixture]
    public class IsEqualTo : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsTrue()
        {
            Assert.IsTrue(_bytes.IsEqualTo(_bytes), "Get IsEqualToTest value");
            Assert.IsTrue(_bytes.IsEqualTo(BitConverter.GetBytes(Value)), "Get IsEqualToTest value");
        }
    }

    [TestFixture]
    public class ToText : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsToTextValue()
        {
            Assert.AreEqual("�[", _bytes.ToText(), "Get ToText value");
        }
    }

    [TestFixture]
    public class Append : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_AppendBytes()
        {
            using(var stream = new MemoryStream(10))
            {
                stream.Append(_bytes);
                Assert.AreEqual(_bytes.Length, stream.Length, "Get Append value");
            }
        }
    }

    [TestFixture]
    public class ReadBytesAsync
    {
        [Test]
        public async Task WithoutBufferSize_ReturnsArray()
        {
            var sampleFile = Path.GetTempFileName();
            Helper.CreateTempBinaryFile(sampleFile, 1);
            Assert.IsTrue(File.Exists(sampleFile));

            var currentAssembly = new FileStream(sampleFile, FileMode.Open);
            var buffer = new byte[100];
            await currentAssembly.ReadAsync(buffer, 0, 100);
            currentAssembly.Position = 0;
            var bufferAsync = await currentAssembly.ReadBytesAsync(100);

            Assert.AreEqual(buffer, bufferAsync);
        }

        [Test]
        public async Task WithoutBufferSizeAndBuffer_ReturnsArray()
        {
            var sampleFile = Path.GetTempFileName();
            Helper.CreateTempBinaryFile(sampleFile, 1);
            Assert.IsTrue(File.Exists(sampleFile));

            var buffer = File.ReadAllBytes(sampleFile);
            var currentAssembly = new FileStream(sampleFile, FileMode.Open);

            var bufferAsync = await currentAssembly.ReadBytesAsync((int)currentAssembly.Length);

            Assert.AreEqual(buffer, bufferAsync);
        }

        [Test]
        public async Task WithBufferSizeAndBuffer_ReturnsArray()
        {
            var sampleFile = Path.GetTempFileName();
            Helper.CreateTempBinaryFile(sampleFile, 1);
            Assert.IsTrue(File.Exists(sampleFile));

            var currentAssembly = new FileStream(sampleFile, FileMode.Open);
            var buffer = new byte[100];
            await currentAssembly.ReadAsync(buffer, 0, 100);
            currentAssembly.Position = 0;
            var bufferAsync = await currentAssembly.ReadBytesAsync(100, 100);

            Assert.AreNotEqual(buffer, bufferAsync);
        }

        [Test]
        public async Task WithBufferSize_ReturnsArray()
        {
            var sampleFile = Path.GetTempFileName();
            Helper.CreateTempBinaryFile(sampleFile, 1);
            Assert.IsTrue(File.Exists(sampleFile));

            var buffer = File.ReadAllBytes(sampleFile);
            var currentAssembly = new FileStream(sampleFile, FileMode.Open);

            var bufferAsync = await currentAssembly.ReadBytesAsync(currentAssembly.Length, 256);

            Assert.AreEqual(buffer, bufferAsync);
        }
    }
}
