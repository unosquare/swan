using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Unosquare.Swan;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ExtensionsByteArraysTest
    {
        static int value = 123456789;
        byte[] bytes = BitConverter.GetBytes(value);

        [TestCase("0x15cd5b07", true)]
        [TestCase("15cd5b07", false)]
        public void ToLowerHexTest(string expected, bool prefix)
        {
            Assert.AreEqual(expected, bytes.ToLowerHex(prefix), $"Get ToLowerHex value");
        }

        [TestCase("0x15CD5B07", true)]
        [TestCase("15CD5B07", false)]
        public void ToUpperHexTest(string expected, bool prefix)
        {
            Assert.AreEqual(expected, bytes.ToUpperHex(prefix), $"Get ToUpperHex value");
        }

        [Test]
        public void ToDashedHexTest()
        {
            Assert.AreEqual("15-CD-5B-07", bytes.ToDashedHex(), $"Get ToDashedHex value");
        }

        [Test]
        public void ToBase64Test()
        {
            Assert.AreEqual("Fc1bBw==", bytes.ToBase64(), $"Get ToBase64 value");
        }

        [Test]
        public void ConvertHexadecimalToBytesTest()
        {
            var hex = "15CD5B07";
            Assert.AreEqual(bytes, hex.ConvertHexadecimalToBytes(), $"Get ConvertHexadecimalToBytes value");
        }

        [Test]
        public void GetBitValueAtTest()
        {
            Assert.AreEqual(0, bytes[0].GetBitValueAt(1), $"Get GetBitValueAt value");
        }

        [Test]
        public void SplitTest()
        {
            List<byte[]> expected = new List<byte[]>() { new byte[] { 91, 7 } };
            byte[] sequence = BitConverter.GetBytes(456);
            Assert.AreEqual(expected, bytes.Split(2, sequence), $"Get Split value");
        }

        [Test]
        public void DeepCloneTest()
        {
            Assert.AreEqual(bytes, bytes.DeepClone(), $"Get DeepClone value");
        }

        [Test]
        public void TrimStartTest()
        {
            Assert.AreEqual(new byte[] { 205, 91, 7 }, bytes.TrimStart(21), $"Get TrimStart value");
        }

        [Test]
        public void TrimEndTest()
        {
            Assert.AreEqual(new byte[] { 21, 205, 91 }, bytes.TrimEnd(7), $"Get TrimEnd value");
        }

        [Test]
        public void TrimTest()
        {
            Assert.AreEqual(new byte[] { 21, 205, 91, 7 }, bytes.Trim(205), $"Get Trim value");
        }

        [TestCase(true, 7)]
        [TestCase(false, 21)]
        public void EndsWithTest(bool expected, byte input)
        {
            Assert.AreEqual(expected, bytes.EndsWith(input), $"Get EndsWith value");
        }

        [TestCase(false, 7)]
        [TestCase(true, 21)]
        public void StartsWithTest(bool expected, byte input)
        {
            Assert.AreEqual(expected, bytes.StartsWith(input), $"Get StartsWith value");
        }

        [TestCase(true, 91)]
        [TestCase(false, 92)]
        public void ContainsTest(bool expected, byte input)
        {
            Assert.AreEqual(expected, bytes.Contains(input), $"Get Contains value");
        }

        [Test]
        public void IsEqualToTest()
        {
            Assert.AreEqual(true, bytes.IsEqualTo(bytes), $"Get Trim value");
        }

        [Test]
        public void ToTextTest()
        {
            Assert.AreEqual("�[", bytes.ToText(), $"Get ToText value");
        }

        [Test]
        public void AppendTest()
        {
            using (MemoryStream stream = new MemoryStream(10))
            {
                stream.Append(bytes);
                Assert.AreEqual(bytes.Length, stream.Length, $"Get Append value");
            }
        }
    }


}
