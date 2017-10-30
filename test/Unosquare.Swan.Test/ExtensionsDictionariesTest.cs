using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Unosquare.Swan.Test.ExtensionsDictionariesTests
{
    public abstract class ExtensionsDictionariesTest
    {
        protected Dictionary<object, object> dict = new Dictionary<object, object>();
    }

    [TestFixture]
    public class GetValueOrDefault : ExtensionsDictionariesTest
    {
        [Test]
        public void NullDictionary_ThrowsArgumentNullException()
        {
            dict = null;

            Assert.Throws<ArgumentNullException>(() =>
            {
                Extensions.GetValueOrDefault(dict, 1);
            });
        }
    }
}
