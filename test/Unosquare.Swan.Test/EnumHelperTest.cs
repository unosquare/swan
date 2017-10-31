using NUnit.Framework;
using System;
using Unosquare.Swan.Components;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test.EnumHelperTest
{
    public abstract class EnumHelperTest
    {
        public enum City
        {
            Stormwind = 1,
            Suramar,
            Orgrimmar,
            Dalaran
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

        [Test]
        public void WithInvalidType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                EnumHelper.GetItemsWithIndex<string>()
            );
        }
    }

    [TestFixture]
    public class GetItemsWithValue : EnumHelperTest
    {
        [Test]
        public void WithValidEnum_ReturnsTuple()
        {
            var arc = EnumHelper.GetItemsWithValue<City>();

            Assert.AreEqual(arc[0].ToString(), "(1, Stormwind)");
            Assert.AreEqual(arc[1].ToString(), "(2, Suramar)");
            Assert.AreEqual(arc[2].ToString(), "(3, Orgrimmar)");
            Assert.AreEqual(arc[3].ToString(), "(4, Dalaran)");
        }

        [Test]
        public void WithInvalidType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                EnumHelper.GetItemsWithValue<string>()
            );
        }

    }

}