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
        protected readonly byte[] Bytes = BitConverter.GetBytes(Value);
        protected readonly byte[] NullBytes = null;
    }

    [TestFixture]
    public class ToLowerHex : ExtensionsByteArraysTest
    {
        [TestCase("0x15cd5b07", true)]
        [TestCase("15cd5b07", false)]
        public void WithValidBytes_ReturnsString(string expected, bool prefix)
        {
            Assert.AreEqual(expected, Bytes.ToLowerHex(prefix), "Get ToLowerHex value");
        }

        [Test]
        public void WithNullBytes_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => NullBytes.ToLowerHex());
        }
    }

    [TestFixture]
    public class ToUpperHex : ExtensionsByteArraysTest
    {
        [TestCase("0x15CD5B07", true)]
        [TestCase("15CD5B07", false)]
        public void WithValidBytes_ReturnsString(string expected, bool prefix)
        {
            Assert.AreEqual(expected, Bytes.ToUpperHex(prefix), "Get ToUpperHex value");
        }

        [Test]
        public void WithNullBytes_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                NullBytes.ToUpperHex()
            );
        }
    }

    [TestFixture]
    public class ToDashedHex : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsString()
        {
            Assert.AreEqual("15-CD-5B-07", Bytes.ToDashedHex(), "Get ToDashedHex value");
        }
    }

    [TestFixture]
    public class ToBase64 : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsString()
        {
            Assert.AreEqual("Fc1bBw==", Bytes.ToBase64(), "Get ToBase64 value");
        }
    }

    [TestFixture]
    public class GetBitValueAt : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsBitValue()
        {
            Assert.AreEqual(0, Bytes[0].GetBitValueAt(1), "Get GetBitValueAt value");
        }
    }

    [TestFixture]
    public class Split : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsSplitedString()
        {
            var expected = new List<byte[]> {new byte[] {91, 7}};
            var sequence = BitConverter.GetBytes(456);
            Assert.AreEqual(expected, Bytes.Split(2, sequence), "Get Split value");
        }

        [Test]
        public void WithSequenceEqualsBytes_ReturnsSplitedString()
        {
            var expected = new List<byte[]> {new byte[] {21, 205, 91, 7}};
            var sequence = BitConverter.GetBytes(123456789);

            Assert.AreEqual(expected, Bytes.Split(0, sequence), "Get Split value");
        }

        [Test]
        public void WithNullSequence_ThrowsArgumentNullException()
        {
            var sequence = BitConverter.GetBytes(456);

            Assert.Throws<ArgumentNullException>(() =>
                NullBytes.Split(2, sequence)
            );
        }

        [Test]
        public void WithNullBytes_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Bytes.Split(2, null)
            );
        }
    }

    [TestFixture]
    public class GetIndexOf : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsNegativeOne()
        {
            Assert.AreEqual(-1, Bytes.GetIndexOf(new byte[0]), "Get index of empty array is -1");
        }

        [Test]
        public void WithNullSequence_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Bytes.GetIndexOf(null)
            );
        }

        [Test]
        public void WithNullBytes_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                NullBytes.GetIndexOf(null)
            );
        }

        [Test]
        public void WithSequenceLongerThanBuffer_ReturnsNegativeOne()
        {
            var bytes = BitConverter.GetBytes(4815162342);

            Assert.AreEqual(-1, Bytes.GetIndexOf(bytes), "Get index of empty array is -1");
        }

        [Test]
        public void WithNegativeOffset_ReturnsNegativeOne()
        {
            var bytes = BitConverter.GetBytes(4815162342);

            Assert.AreEqual(-1, bytes.GetIndexOf(Bytes, -1), "Get index of empty array is -1");
        }
    }

    [TestFixture]
    public class DeepClone : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsArray()
        {
            Assert.AreEqual(Bytes, Bytes.DeepClone(), "Get DeepClone value");
        }

        [Test]
        public void WithNullBytes_ThrowsArgumentNullException()
        {
            Assert.IsNull(NullBytes.DeepClone());
        }
    }

    [TestFixture]
    public class Trim : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsTrimValue()
        {
            Assert.AreEqual(new byte[] {21, 205, 91, 7}, Bytes.Trim(205), "Get Trim value");
        }
    }

    [TestFixture]
    public class TrimStart : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsTrimStartValue()
        {
            Assert.AreEqual(new byte[] {205, 91, 7}, Bytes.TrimStart(21), "Get TrimStart value");
        }

        [Test]
        public void WithNullBytes_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                NullBytes.TrimStart(21)
            );
        }
    }

    [TestFixture]
    public class TrimEnd : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsTrimEndValue()
        {
            Assert.AreEqual(new byte[] {21, 205, 91}, Bytes.TrimEnd(7), "Get TrimEnd value");
        }

        [Test]
        public void WithNullBytes_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                NullBytes.TrimEnd(7)
            );
        }
    }

    [TestFixture]
    public class EndsWith : ExtensionsByteArraysTest
    {
        [TestCase(true, 7)]
        [TestCase(false, 21)]
        public void WithValidBytes_ReturnsEndsWithValue(bool expected, byte input)
        {
            Assert.AreEqual(expected, Bytes.EndsWith(input), "Get EndsWith value");
        }

        [Test]
        public void WithNullBytes_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                NullBytes.EndsWith(7)
            );
        }
    }

    [TestFixture]
    public class StartsWith : ExtensionsByteArraysTest
    {
        [TestCase(false, 7)]
        [TestCase(true, 21)]
        public void WithValidBytes_ReturnsStartsWithValue(bool expected, byte input)
        {
            Assert.AreEqual(expected, Bytes.StartsWith(input), "Get StartsWith value");
        }
    }

    [TestFixture]
    public class Contains : ExtensionsByteArraysTest
    {
        [TestCase(true, 91)]
        [TestCase(false, 92)]
        public void WithValidBytes_ReturnsContainsValue(bool expected, byte input)
        {
            Assert.AreEqual(expected, Bytes.Contains(input), "Get Contains value");
        }
    }

    [TestFixture]
    public class IsEqualTo : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsTrue()
        {
            Assert.IsTrue(Bytes.IsEqualTo(Bytes), "Get IsEqualToTest value");
            Assert.IsTrue(Bytes.IsEqualTo(BitConverter.GetBytes(Value)), "Get IsEqualToTest value");
        }

        [Test]
        public void WithNullBytes_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                NullBytes.IsEqualTo(BitConverter.GetBytes(Value))
            );
        }
    }

    [TestFixture]
    public class ToText : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_ReturnsToTextValue()
        {
            Assert.AreEqual("�[", Bytes.ToText(), "Get ToText value");
        }
    }

    [TestFixture]
    public class Append : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidBytes_AppendBytes()
        {
            using (var stream = new MemoryStream(10))
            {
                stream.Append(Bytes);
                Assert.AreEqual(Bytes.Length, stream.Length, "Get Append value");
            }
        }

        [Test]
        public void WithNullBuffer_ThrowsArgumentNullException()
        {
            using (var stream = new MemoryStream(10))
            {
                Assert.Throws<ArgumentNullException>(() =>
                    stream.Append(NullBytes)
                );
            }
        }

        [Test]
        public void WithNullStream_ThrowsArgumentNullException()
        {
            using (MemoryStream stream = null)
            {
                Assert.Throws<ArgumentNullException>(() =>
                    stream.Append(NullBytes)
                );
            }
        }

        [Test]
        public void WithValidIEnumerable_AppendBytes()
        {
            IEnumerable<byte> enumerableByte = BitConverter.GetBytes(Value);

            using (var stream = new MemoryStream(10))
            {
                stream.Append(enumerableByte);

                Assert.AreEqual(4, stream.Length, "Get Append value");
            }
        }

        [Test]
        public void WithNullIEnumerable_AppendBytes()
        {
            IEnumerable<byte> enumerableByte = null;

            using (var stream = new MemoryStream(10))
            {
                Assert.Throws<ArgumentNullException>(() =>
                    stream.Append(enumerableByte)
                );
            }
        }

        [Test]
        public void WithValidIEnumerableArray_AppendBytes()
        {
            IEnumerable<byte[]> enumerableByte = null;

            using (var stream = new MemoryStream(10))
            {
                Assert.Throws<ArgumentNullException>(() =>
                    stream.Append(enumerableByte)
                );
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
            Helper.CreateTempBinaryFile(sampleFile);
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
            Helper.CreateTempBinaryFile(sampleFile);
            Assert.IsTrue(File.Exists(sampleFile));

            var buffer = File.ReadAllBytes(sampleFile);
            var currentAssembly = new FileStream(sampleFile, FileMode.Open);

            var bufferAsync = await currentAssembly.ReadBytesAsync((int) currentAssembly.Length);

            Assert.AreEqual(buffer, bufferAsync);
        }

        [Test]
        public async Task WithBufferSizeAndBuffer_ReturnsArray()
        {
            var sampleFile = Path.GetTempFileName();
            Helper.CreateTempBinaryFile(sampleFile);
            Assert.IsTrue(File.Exists(sampleFile));

            var currentAssembly = new FileStream(sampleFile, FileMode.Open);
            var buffer = new byte[100];
            await currentAssembly.ReadAsync(buffer, 0, 100);
            currentAssembly.Position = 0;
            var bufferAsync = await currentAssembly.ReadBytesAsync(100, 100);

            Assert.AreEqual(buffer, bufferAsync);
        }

        [TestCase(256)]
        [TestCase(25654323)]
        public async Task WithBufferSize_ReturnsArray(int bufferLength)
        {
            var sampleFile = Path.GetTempFileName();
            Helper.CreateTempBinaryFile(sampleFile);
            Assert.IsTrue(File.Exists(sampleFile));

            var buffer = File.ReadAllBytes(sampleFile);
            var currentAssembly = new FileStream(sampleFile, FileMode.Open);

            var bufferAsync = await currentAssembly.ReadBytesAsync(currentAssembly.Length, bufferLength);

            Assert.AreEqual(buffer, bufferAsync);
        }

        [Test]
        public void WithNullStreamAndBufferLength_ThrowsArgumentNullException()
        {
            FileStream currentAssembly = null;

            Assert.ThrowsAsync<ArgumentNullException>(async () =>

                await currentAssembly.ReadBytesAsync(23, 256)
            );
        }

        [Test]
        public void WithNullStream_ThrowsArgumentNullException()
        {
            FileStream currentAssembly = null;

            Assert.ThrowsAsync<ArgumentNullException>(async () =>

                await currentAssembly.ReadBytesAsync(23)
            );
        }

    }

    [TestFixture]
    public class SetBitValueAt : ExtensionsByteArraysTest
    {
        [Test]
        public void WithOffsetAndValue_ReturnsBitValue()
        {
            byte input = 201;

            var result = input.SetBitValueAt(2, 1);

            Assert.AreEqual(205, result);
        }
    }

    [TestFixture]
    public class ConvertHexadecimalToBytes : ExtensionsByteArraysTest
    {
        [Test]
        public void WithValidHex_ReturnsString()
        {
            const string hex = "15CD5B07";

            Assert.AreEqual(Bytes, hex.ConvertHexadecimalToBytes(), "Get ConvertHexadecimalToBytes value");
        }

        [Test]
        public void WithNullHex_ThrowsArgumentNullException()
        {
            const string hex = null;

            Assert.Throws<ArgumentNullException>(() =>
                hex.ConvertHexadecimalToBytes()
            );
        }
    }

    [TestFixture]
    public class SubArray : ExtensionsByteArraysTest
    {
        [TestCase(0, 2, 21)]
        [TestCase(2, 2, 91)]
        public void WithValidParams_ReturnsSubArray(long startIndex, long length, int expected)
        {
            var sub = Bytes.SubArray(startIndex, length);

            Assert.AreEqual(expected, sub[0]);
        }

        [TestCase(2, 3)]
        [TestCase(2, 0)]
        [TestCase(-1, 3)]
        public void WithInvalidParams_ReturnsEmpty(long startIndex, long length)
        {
            var sub = Bytes.SubArray(startIndex, length);

            Assert.IsEmpty(sub);
        }

        [Test]
        public void WithNullArray_ReturnsEmpty()
        {
            long startIndex = 0;
            long length = 0;

            var sub = NullBytes.SubArray(startIndex, length);

            Assert.IsEmpty(sub);
        }
    }

    [TestFixture]
    public class ToByteArray : ExtensionsByteArraysTest
    {
        [Test]
        public void WithNullArray_ReturnsByteArray()
        {
            sbyte[] input = null;

            Assert.Throws<ArgumentNullException>(() =>
                input.ToByteArray()
            );
        }
    }

    [TestFixture]
    public class ToSByteArray : ExtensionsByteArraysTest
    {
        [Test]
        public void WithNullArray_ReturnsByteArray()
        {
            Assert.Throws<ArgumentNullException>(() =>
                NullBytes.ToSByteArray()
            );
        }
    }

    [TestFixture]
    public class ReadInput
    {
        [Test]
        public void WithNullStream_ThrowsArgumentNullException()
        {
            FileStream stream = null;
            var lber = new sbyte[23];

            Assert.Throws<ArgumentNullException>(() =>
                stream.ReadInput(ref lber, 0, lber.Length)
            );
        }

        [Test]
        public void WithTargetLengthEqualsZero_ReturnsZeroBytes()
        {
            var sampleFile = Path.GetTempFileName();
            var stream = new FileStream(sampleFile, FileMode.Open);
            var lber = new sbyte[0];

            var result = stream.ReadInput(ref lber, 0, lber.Length);

            Assert.AreEqual(0, result);
        }

        [Test]
        public void WithCountEqualsZero_ReturnsNegativeOne()
        {
            var sampleFile = Path.GetTempFileName();
            var stream = new FileStream(sampleFile, FileMode.Open);
            var lber = new sbyte[234];

            var result = stream.ReadInput(ref lber, 0, 0);

            Assert.AreEqual(-1, result);
        }

        [Test]
        public void WithNullTarget_ThrowsArgumentNullException()
        {
            var sampleFile = Path.GetTempFileName();
            var stream = new FileStream(sampleFile, FileMode.Open);
            sbyte[] lber = null;

            Assert.Throws<ArgumentNullException>(() =>
                stream.ReadInput(ref lber, 0, 0)
            );
        }
    }
}