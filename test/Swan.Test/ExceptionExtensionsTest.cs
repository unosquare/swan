using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Swan.Test
{
    [TestFixture]
    public class ExceptionExtensionsTest
    {
        [Test]
        public void IsCriticalFatalTestFalse()
        {
            var ex = new Exception();
            Assert.That(!ex.IsCriticalException());
            Assert.That(!ex.IsFatalException());
        }

        [Test]
        public void IsCriticalExceptionTrue()
        {
            var ex = new AppDomainUnloadedException();
            Assert.That(ex.IsCriticalException);
        }

        [Test]
        public void IsFatalTrue()
        {
            var ex = new OutOfMemoryException();
            Assert.That(ex.IsCriticalException);
        }

        [Test]
        public void InCriticalInner()
        {
            var ex = new Exception(string.Empty, new AppDomainUnloadedException());
            Assert.That(ex.IsCriticalException);

            var ex1 = new Exception(string.Empty, new Exception());
            Assert.That(!ex1.IsCriticalException());
        }

        [Test]
        public void InFatalInner()
        {
            var ex = new Exception(string.Empty, new OutOfMemoryException());
            Assert.That(ex.IsCriticalException);

            var ex1 = new Exception(string.Empty, new Exception());
            Assert.That(!ex1.IsCriticalException());
        }
    }
}
