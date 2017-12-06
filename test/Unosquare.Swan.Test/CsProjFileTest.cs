namespace Unosquare.Swan.Test
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using NUnit.Framework;
    using Unosquare.Swan.Components;
    using Unosquare.Swan.Test.Mocks;
    using System;

    public abstract class CsProjFileTest : TestFixtureBase
     {
        protected string _data = @"<Project Sdk=""Microsoft.NET.Sdk"">
                                  <PropertyGroup>
                                    <Description>Unit Testing project</Description>
                                    <Copyright>Copyright(c) 2016-2017 - Unosquare</Copyright>
                                    <AssemblyTitle>Unosquare SWAN Test</AssemblyTitle>
                                    <TargetFrameworks>net46;netcoreapp2.0</TargetFrameworks>
                                    <AssemblyName>Unosquare.Swan.Test</AssemblyName>
                                    <DebugType>Full</DebugType>
                                  </PropertyGroup></Project>";
        protected string _wrongSDK = @"<Project Sdk=""Microhard.NET.Sdk""></Project>";
     }

    [TestFixture]
    public class CsProjFileConstructor : CsProjFileTest 
    {
        [Test]
        public void WithValidFileAndValidClass_ReturnsFileAndMetadata()
        {
                using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
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
                using (var csproj = new CsProjFile<CsMetadataMock>(stream)) { }
            });
        }

        [Test]
        public void IfPropertyWasNotFound_ReturnsNull()
        {
            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
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
                using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_wrongSDK)))
                using (var csproj = new CsProjFile<CsMetadataMock>(stream)){}
            });
        }

        [Test]
        public void WithAbstractClass_ThrowsMissingMethodException()
        {               
            Assert.Throws<MissingMethodException>(() =>
            {
                using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(_data)))
                using (var csproj = new CsProjFile<CsAbstractMetadataMock>(stream)) { }
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
    }
}
