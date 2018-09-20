namespace Unosquare.Swan.Test
{
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;
    using Components;
    using Mocks;

    [TestFixture]
    public class CsProjFileConstructor
    {
        private const string Data = @"<Project Sdk=""Microsoft.NET.Sdk"">
                                  <PropertyGroup>
                                    <Description>Unit Testing project</Description>
                                    <Copyright>Copyright(c) 2016-2018 - Unosquare</Copyright>
                                    <AssemblyTitle>Unosquare SWAN Test</AssemblyTitle>
                                    <TargetFrameworks>net46;netcoreapp2.1</TargetFrameworks>
                                    <AssemblyName>Unosquare.Swan.Test</AssemblyName>
                                    <DebugType>Full</DebugType>
                                  </PropertyGroup></Project>";

        private const string WrongSdk = @"<Project Sdk=""Microhard.NET.Sdk""></Project>";

        [Test]
        public void WithValidFileAndValidClass_ReturnsFileAndMetadata()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data)))
            using (var csproj = new CsProjFile<CsMetadataMock>(stream))
            {
                Assert.IsNotNull(csproj);
                Assert.IsNotNull(csproj.Metadata);
                Assert.IsNotNull(csproj.Metadata.Copyright);
            }
        }

        [Test]
        public void WithNullStream_ThrowsXmlException()
        {
            Assert.Throws<XmlException>(() =>
            {
                using (var stream = new MemoryStream())
                using (var csproj = new CsProjFile<CsMetadataMock>(stream))
                {
                }
            });
        }

        [Test]
        public void IfPropertyWasNotFound_ReturnsNull()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data)))
            using (var csproj = new CsProjFile<CsMetadataMock>(stream))
            {
                Assert.IsNull(csproj.Metadata.NonExistentProp);
            }
        }

        [Test]
        public void WithWrongSdk_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(WrongSdk)))
                using (var csproj = new CsProjFile<CsMetadataMock>(stream))
                {
                }
            });
        }

        [Test]
        public void WithAbstractClass_ThrowsMissingMethodException()
        {
            Assert.Throws<MissingMethodException>(() =>
            {
                using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data)))
                using (var csproj = new CsProjFile<CsAbstractMetadataMock>(stream))
                {
                }
            });
        }

        [Test]
        public void WithEmptyStringAsFileName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var csproj = new CsProjFile<CsAbstractMetadataMock>(string.Empty);
            });
        }

        [Test]
        public void WithNullAsFileName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(Path.GetTempPath());
                var csproj = new CsProjFile<CsMetadataMock>();
                Directory.SetCurrentDirectory(currentDirectory);
            });
        }

        [TestCase("sample.fsproj")]
        [TestCase("sample.csproj")]
        public void WithTempFileAndValidClass_ReturnsFileAndMetadata(string projectFilename)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(Path.GetTempPath());
            File.WriteAllText(Path.Combine(Path.GetTempPath(), $"{DateTime.Now.Second}_{DateTime.Now.Millisecond}_{projectFilename}"), Data);

            using (var csproj = new CsProjFile<CsMetadataMock>())
            {
                Assert.IsNotNull(csproj);
                Assert.IsNotNull(csproj.Metadata);
                Assert.IsNotNull(csproj.Metadata.Copyright);
            }

            Directory.SetCurrentDirectory(currentDirectory);
        }
    }
}