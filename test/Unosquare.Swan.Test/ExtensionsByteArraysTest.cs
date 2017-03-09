using NUnit.Framework;
using System;
using System.Collections.Generic;
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
            Assert.AreEqual(0, bytes[0].GetBitValueAt(1), $"Get ConvertHexadecimalToBytes value");
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
    }


}
