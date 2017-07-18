using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Unosquare.Swan.Components;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ObjectComparerTest
    {

        [Test]
        public void CompareEqualsObjectsTest()
        {
            var left = DateBasicJson.GetDateDefault();
            var right = DateBasicJson.GetDateDefault();

            Assert.IsTrue(ObjectComparer.AreEqual(left, right));
        }

        [Test]
        public void CompareDifferentObjectsTest()
        {
            var left = BasicJson.GetDefault();
            var right = new BasicJson();

            Assert.IsFalse(ObjectComparer.AreEqual(left, right));
        }

        [Test]
        public void CompareObjectsWithArray()
        {
            var first = new[] { BasicJson.GetDefault() };
            var second = new[] { BasicJson.GetDefault() };

            Assert.IsTrue(ObjectComparer.AreEqual(first, second));
        }

        [Test]
        public void CompareObjectsWithArrayObject()
        {
            var first = new AdvArrayJson { Id = 1, Properties = new[] { BasicJson.GetDefault() } };
            var second = new AdvArrayJson { Id = 1, Properties = new[] { BasicJson.GetDefault() } };

            Assert.IsTrue(ObjectComparer.AreEqual(first, second));
        }

    }
}
