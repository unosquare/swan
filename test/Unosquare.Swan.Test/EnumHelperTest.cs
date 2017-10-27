using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.Swan.Components;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test.EnumHelperTest
{
    [TestFixture]
    public class GetItemsWithValue
    {
        [Test]
        public void WithValidEnum_ReturnsTuple()
        {
            var arc = EnumHelper.GetItemsWithValue<MyEnum>();
            
            Assert.AreEqual(arc[0].ToString(), "(0, One)");
            Assert.AreEqual(arc[1].ToString(), "(1, Two)");
            Assert.AreEqual(arc[2].ToString(), "(2, Three)");
        }
    }

    [TestFixture]
    public class GetItemsWithIndex
    {
        [Test]
        public void WithValidEnum_ReturnsTuple()
        {
            var arc = EnumHelper.GetItemsWithIndex<MyEnum>();

            Assert.AreEqual(arc[0].ToString(), "(0, One)");
            Assert.AreEqual(arc[1].ToString(), "(1, Two)");
            Assert.AreEqual(arc[2].ToString(), "(2, Three)");
        }
    }

}