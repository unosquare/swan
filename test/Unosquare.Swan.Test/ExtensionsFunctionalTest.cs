using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test.ExtensionsFunctionalTest
{
    public abstract class ExtensionsFunctionalTest
    {
        protected readonly IEnumerable<byte> enumerableByte = BitConverter.GetBytes(123456789);

        protected bool ReturnTrue()
        {
            return true;
        }

        protected bool ReturnFalse()
        {
            return false;
        }

        protected IEnumerable<byte> Arc(IEnumerable<byte> input)
        {
            IEnumerable<byte> enumerableByte = BitConverter.GetBytes(45347645);

            return enumerableByte;
        }
    }

    [TestFixture]
    public class When : ExtensionsFunctionalTest
    {
        [Test]
        public void WithMethodCallEqualsTrue_IEnumerable()
        {
            Func<bool> methodCall = ReturnTrue;
            Func<IEnumerable<byte>, IEnumerable<byte>> dsad = Arc;
            
            var whenResult = enumerableByte.When(methodCall, dsad);
            
            Assert.AreNotEqual(whenResult, enumerableByte);
        }

        [Test]
        public void WithMethodCallEqualsFalse_IEnumerable()
        {
            Func<bool> methodCall = ReturnFalse;
            Func<IEnumerable<byte>, IEnumerable<byte>> dsad = Arc;

            var whenResult = enumerableByte.When(methodCall, dsad);

            Assert.AreEqual(whenResult, enumerableByte);
        }

    }
   
}
