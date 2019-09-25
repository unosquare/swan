using NUnit.Framework;
using Swan.Cryptography;
using System;
using System.IO;

namespace Swan.Test
{
    [TestFixture]
    public class HasherTest
    {
        [TestFixture]
        public class ComputeMD5 : ExtensionsByteArraysTest.ExtensionsByteArraysTest
        {
            [TestCase("6B-F9-5A-48-F3-66-BD-F8-AF-3A-19-8C-7B-72-3C-77", 5000)]
            [TestCase("9B-4C-8A-5E-36-D3-BE-7E-2C-4B-1D-75-DE-D8-C8-A1", 1234)]
            [TestCase("45-9B-B4-0F-7B-36-1B-90-41-4A-72-D4-0B-5A-0E-D5", 53454)]
            public void WithValidStream_ReturnsMD5(string expected, int stream)
            {
                using var input = new MemoryStream(new byte[stream]);
                Assert.AreEqual(expected, Hasher.ComputeMD5(input).ToDashedHex());
            }

            [TestCase("50-82-83-2F-01-E0-EB-94-30-C9-DB-6E-AE-FE-BC-72", "Illidan")]
            [TestCase("8A-B3-AF-D6-A4-82-35-06-10-78-20-FA-EB-85-5E-B4", "Arthas")]
            [TestCase("8C-1D-5F-69-43-84-06-3D-F2-E0-66-4F-54-9C-4B-93", "Grommash")]
            public void WithValidString_ReturnsMD5(string expected, string input)
            {
                Assert.AreEqual(expected, Hasher.ComputeMD5(input).ToDashedHex(), "Get MD5");
            }

            [Test]
            public void WithNull_ReturnsMD5()
            {
                Assert.Throws<ArgumentNullException>(() => Hasher.ComputeMD5(NullMemoryStream));
            }
        }

        [TestFixture]
        public class ComputeSha1
        {
            [TestCase("06636F8D82BDEB41C444F82D2EBCF431FC31FE12", "Suramar")]
            [TestCase("0E3EB0AF296788BC24DD29BC3C767EE6A829D473", "Stormwind")]
            [TestCase("D4570F48B4B7B720B55499B5D01A0215A6A60FB2", "Darnassus")]
            public void WithValidString_ReturnsSha1(string expected, string input)
            {
                Assert.AreEqual(expected, Hasher.ComputeSha1(input).ToUpperHex(), "Get Sha1");
            }
        }

        [TestFixture]
        public class ComputeSha256
        {
            [Test]
            public void WithValidString_ReturnsSha256()
            {
                const string input = "HOLA";

                Assert.IsTrue(
                    "73C3DE4175449987EF6047F6E0BEA91C1036A8599B43113B3F990104AB294A47".ConvertHexadecimalToBytes() == Hasher.ComputeSha256(input));
            }
        }

        [TestFixture]
        public class ComputeSha512
        {
            [TestCase("uG16jy5/N+hPwel+4xRVtOfyCZ56K9Ds0SF4GE9oQgYRBGzTeAD+h94cIgc6ROyNjbK6wBFVhgqqjDDh01f4rg==",
                "Eastern Kingdoms")]
            [TestCase("rDh3voP2/h+S/mDAjnsf8MFRM+Hst6mTxB+rxehSA2KW5fUR2hSBNO9AGGifOzUuWPrO0OOpE0nskGPUw2q+iQ==",
                "Northrend")]
            [TestCase("f1WQUrni0kEMcQ0u8tYkvC17zWphJYzQqdWHsrXRuUBoSG+MUrLx3urczB+zkJ9OFbCqUyjCV6NqMEViv7drqg==",
                "Pandaria")]
            public void WithValidString_ReturnsSha512(string expected, string input)
            {
                Assert.AreEqual(expected, Hasher.ComputeSha512(input).ToBase64(), "Get Sha512");
            }
        }
    }
}
