using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Unosquare.Swan.Attributes;

namespace Unosquare.Swan.Test.ArgumentOptionAttributeTests
{
    public abstract class ArgumentOptionAttributeTest
    {
        protected ArgumentOptionAttribute argOptAttr;
    }

    [TestFixture]
    public class Constructor : ArgumentOptionAttributeTest
    {
        [Test]
        public void NotInitialized_Valid()
        {
            argOptAttr = new ArgumentOptionAttribute();

            Assert.IsNotNull(argOptAttr);
        }
    }
}
