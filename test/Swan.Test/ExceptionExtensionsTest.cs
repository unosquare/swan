using NUnit.Framework;
using Swan.Reflection;
using System;

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
        [TestCase(typeof(AppDomainUnloadedException))]
        [TestCase(typeof(BadImageFormatException))]
        [TestCase(typeof(CannotUnloadAppDomainException))]
        [TestCase(typeof(InvalidProgramException))]
        [TestCase(typeof(NullReferenceException))]
        public void IsCriticalExceptionTrue(Type type)
        {
            var ex = TypeManager.CreateInstance(type) as Exception;
            Assert.That(ex.IsCriticalException);
        }

        [Test]
        [TestCase(typeof(StackOverflowException))]
        [TestCase(typeof(OutOfMemoryException))]
        [TestCase(typeof(AccessViolationException))]
        public void IsFatalTrue(Type type)
        {
            var ex = TypeManager.CreateInstance(type) as Exception;
            Assert.That(ex.IsCriticalException);
        }

        [Test]
        [TestCase(typeof(AppDomainUnloadedException))]
        [TestCase(typeof(BadImageFormatException))]
        [TestCase(typeof(CannotUnloadAppDomainException))]
        [TestCase(typeof(InvalidProgramException))]
        [TestCase(typeof(NullReferenceException))]
        public void InCriticalInner(Type type)
        {
            var innerEx = TypeManager.CreateInstance(type) as Exception;
            var ex = new Exception(string.Empty, innerEx);
            Assert.That(ex.IsCriticalException);

            var ex1 = new Exception(string.Empty, new Exception());
            Assert.That(!ex1.IsCriticalException());
        }

        [Test]
        [TestCase(typeof(StackOverflowException))]
        [TestCase(typeof(OutOfMemoryException))]
        [TestCase(typeof(AccessViolationException))]
        public void InFatalInner(Type type)
        {
            var innerEx = TypeManager.CreateInstance(type) as Exception;
            var ex = new Exception(string.Empty, innerEx);
            Assert.That(ex.IsCriticalException);

            var ex1 = new Exception(string.Empty, new Exception());
            Assert.That(!ex1.IsCriticalException());
        }
    }
}