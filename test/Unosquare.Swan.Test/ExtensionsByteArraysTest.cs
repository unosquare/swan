using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ExtensionsByteArraysTest
    {
        private const int Value = 123456789;
        private readonly byte[] _bytes = BitConverter.GetBytes(Value);
        
        [TestCase("0x15cd5b07", true)]
        [TestCase("15cd5b07", false)]
        public void ToLowerHexTest(string expected, bool prefix)
        {
            Assert.AreEqual(expected, _bytes.ToLowerHex(prefix), "Get ToLowerHex value");
        }

        [TestCase("0x15CD5B07", true)]
        [TestCase("15CD5B07", false)]
        public void ToUpperHexTest(string expected, bool prefix)
        {
            Assert.AreEqual(expected, _bytes.ToUpperHex(prefix), $"Get ToUpperHex value");
        }

        [Test]
        public void ToDashedHexTest()
        {
            Assert.AreEqual("15-CD-5B-07", _bytes.ToDashedHex(), $"Get ToDashedHex value");
        }

        [Test]
        public void ToBase64Test()
        {
            Assert.AreEqual("Fc1bBw==", _bytes.ToBase64(), $"Get ToBase64 value");
        }

        [Test]
        public void ConvertHexadecimalToBytesTest()
        {
            const string hex = "15CD5B07";
            Assert.AreEqual(_bytes, hex.ConvertHexadecimalToBytes(), $"Get ConvertHexadecimalToBytes value");
        }

        [Test]
        public void GetBitValueAtTest()
        {
            Assert.AreEqual(0, _bytes[0].GetBitValueAt(1), $"Get GetBitValueAt value");
        }

        [Test]
        public void SplitTest()
        {
            var expected = new List<byte[]>() { new byte[] { 91, 7 } };
            var sequence = BitConverter.GetBytes(456);
            Assert.AreEqual(expected, _bytes.Split(2, sequence), $"Get Split value");
        }

        [Test]
        public void TestSplitExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => _bytes.Split(2, null));
        }
        
        [Test]
        public void TestGetIndexOf()
        {
            Assert.AreEqual(-1, _bytes.GetIndexOf(new byte[0]), "Get index of empty array is -1");
        }

        [Test]
        public void TestGetIndexOfExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => _bytes.GetIndexOf(null));
        }

        [Test]
        public void DeepCloneTest()
        {
            Assert.AreEqual(_bytes, _bytes.DeepClone(), "Get DeepClone value");
        }

        [Test]
        public void TrimStartTest()
        {
            Assert.AreEqual(new byte[] { 205, 91, 7 }, _bytes.TrimStart(21), "Get TrimStart value");
        }

        [Test]
        public void TrimEndTest()
        {
            Assert.AreEqual(new byte[] { 21, 205, 91 }, _bytes.TrimEnd(7), "Get TrimEnd value");
        }

        [Test]
        public void TrimTest()
        {
            Assert.AreEqual(new byte[] { 21, 205, 91, 7 }, _bytes.Trim(205), "Get Trim value");
        }

        [TestCase(true, 7)]
        [TestCase(false, 21)]
        public void EndsWithTest(bool expected, byte input)
        {
            Assert.AreEqual(expected, _bytes.EndsWith(input), "Get EndsWith value");
        }

        [TestCase(false, 7)]
        [TestCase(true, 21)]
        public void StartsWithTest(bool expected, byte input)
        {
            Assert.AreEqual(expected, _bytes.StartsWith(input), "Get StartsWith value");
        }

        [TestCase(true, 91)]
        [TestCase(false, 92)]
        public void ContainsTest(bool expected, byte input)
        {
            Assert.AreEqual(expected, _bytes.Contains(input), "Get Contains value");
        }

        [Test]
        public void IsEqualToTest()
        {
            Assert.IsTrue(_bytes.IsEqualTo(_bytes), "Get IsEqualToTest value");
            Assert.IsTrue(_bytes.IsEqualTo(BitConverter.GetBytes(Value)), "Get IsEqualToTest value");
        }

        [Test]
        public void ToTextTest()
        {
            Assert.AreEqual("�[", _bytes.ToText(), "Get ToText value");
        }

        [Test]
        public void AppendTest()
        {
            using (var stream = new MemoryStream(10))
            {
                stream.Append(_bytes);
                Assert.AreEqual(_bytes.Length, stream.Length, "Get Append value");
            }
        }

        [Test]
        public async Task ReadBytesAsyncWithBuffersizePartialTest()
        {
            var sampleFile = Path.GetTempFileName();
            Helper.CreateTempBinaryFile(sampleFile, 1);
            Assert.IsTrue(File.Exists(sampleFile));

            var currentAssembly = new FileStream(sampleFile, FileMode.Open);
            var buffer = new byte[100];
            await currentAssembly.ReadAsync(buffer, 0, 100);
            currentAssembly.Position = 0;
            var bufferAsync = await currentAssembly.ReadBytesAsync(100, 100);

            Assert.AreEqual(buffer, bufferAsync);
        }

        [Test]
        public async Task ReadBytesAsyncPartialTest()
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
        public async Task ReadBytesAsyncFullTest()
        {
            var sampleFile = Path.GetTempFileName();
            Helper.CreateTempBinaryFile(sampleFile, 1);
            Assert.IsTrue(File.Exists(sampleFile));

            var buffer = File.ReadAllBytes(sampleFile);
            var currentAssembly = new FileStream(sampleFile, FileMode.Open);
            
            var bufferAsync = await currentAssembly.ReadBytesAsync((int) currentAssembly.Length);

            Assert.AreEqual(buffer, bufferAsync);
        }

        [Test]
        public async Task ReadBytesAsyncWithBuffersizeFullTest()
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
