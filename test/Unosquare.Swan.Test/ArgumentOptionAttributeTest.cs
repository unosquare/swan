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

        [Test]
        public void InitializedWithACharacter_Valid()
        {
            argOptAttr = new ArgumentOptionAttribute('U');

            Assert.IsNotNull(argOptAttr);
        }

        [Test]
        public void InitializedWithAString_Valid()
        {
            argOptAttr = new ArgumentOptionAttribute("UnoSquare");

            Assert.IsNotNull(argOptAttr);
        }

        [Test]
        public void InitializedWithACharacterAndString_Valid()
        {
            argOptAttr = new ArgumentOptionAttribute('U', "UnoSquare");

            Assert.IsNotNull(argOptAttr);
        }
    }
}
