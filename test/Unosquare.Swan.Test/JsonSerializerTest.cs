using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unosquare.Swan.Formatters;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test.JsonSerializerTest
{
    public abstract class JsonSerializerTest
    {

    }
    
    [TestFixture]
    public class ResolveBasicType
    {
        [Test]
        public void WithType_ReturnsString()
        {
            var dataSerialized = Json.SerializeOnly(typeof(string), true, null);
            
            Assert.AreEqual("\"System.String\"", dataSerialized);
        }
    }
    
    [TestFixture]
    public class ResolveDictionary
    {
        [Test]
        public void WithEmptyDictionary_ReturnsString()
        {
            var emptyDictionary = new Dictionary<string, string>();

            var dataSerialized = Json.SerializeOnly(emptyDictionary, true, null);

            Assert.AreEqual("{ }", dataSerialized);
        }
        
    }

    [TestFixture]
    public class ResolveEnumerable : JsonSerializerTest
    {
        [Test]
        public void WithEmptyEnumerable_ReturnsString()
        {
            var emptyEnumerable = Enumerable.Empty<int>();

            var dataSerialized = Json.SerializeOnly(emptyEnumerable, true, null);

            Assert.AreEqual("[ ]", dataSerialized);
            Assert.IsNotNull(null);
        }
    }
}