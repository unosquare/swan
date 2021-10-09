namespace Swan.Test.MethodInfoCacheTest
{
    using NUnit.Framework;
    using Swan.Reflection;
    using Swan.Test.Mocks;
    using System;

    [TestFixture]
    public class Retrieve
    {
        private static readonly MethodInfoCache Cache = new();

        [Test]
        public void SingleMethodWithType_ReturnsMethodInfo()
        {
            var methodInfo = Cache.Retrieve(typeof(MethodCacheMock), nameof(MethodCacheMock.SingleMethod));

            Assert.NotNull(methodInfo);
        }

        [Test]
        public void SingleMethodWithGenericType_ReturnsMethodInfo()
        {
            var methodInfo = Cache.Retrieve<MethodCacheMock>(nameof(MethodCacheMock.SingleMethod));

            Assert.NotNull(methodInfo);
        }

        [Test]
        public void MultiMethodWithGenericTypeAndParamType_ReturnsMethodInfo()
        {
            var methodInfo = Cache.Retrieve<MethodCacheMock>(nameof(MethodCacheMock.MultiMethod), typeof(int));

            Assert.NotNull(methodInfo);
        }

        [Test]
        public void AmbiguousMethodWithTypeNoParamType_ThrowsAmbiguousMatchException()
        {
            Assert.Throws<System.Reflection.AmbiguousMatchException>(() =>
                Cache.Retrieve(typeof(MethodCacheMock), nameof(MethodCacheMock.AmbiguousMethod)));
        }

        [Test]
        public void MultiMethodWithGenericTypeAndWithDifferentParamType_ReturnSameMethodInfo()
        {
            var methodInfoIntParam = Cache.Retrieve(typeof(MethodCacheMock), nameof(MethodCacheMock.MultiMethod), typeof(int));
            var methodInfoDecimalParam = Cache.Retrieve(typeof(MethodCacheMock), nameof(MethodCacheMock.MultiMethod), typeof(decimal));

            Assert.AreEqual(methodInfoIntParam, methodInfoDecimalParam);
        }

        [Test]
        public void MultiMethodWithAliasWithTypeAndWithDifferentParamType_ReturnDifferentMethodInfo()
        {
            var methodInfoIntParam = Cache.Retrieve<MethodCacheMock>(nameof(MethodCacheMock.MultiMethod), "multiintgeneric", typeof(int));
            var methodInfoDecimalParam = Cache.Retrieve<MethodCacheMock>(nameof(MethodCacheMock.MultiMethod), "multidecimalgeneric", typeof(decimal));

            Assert.AreNotEqual(methodInfoIntParam, methodInfoDecimalParam);
        }

        [Test]
        public void MultiMethodWithAliasWithGenericTypeAndWithDifferentParamType_ReturnDifferentMethodInfo()
        {
            var methodInfoIntParam = Cache.Retrieve(typeof(MethodCacheMock), nameof(MethodCacheMock.MultiMethod), "multiint", typeof(int));
            var methodInfoDecimalParam = Cache.Retrieve(typeof(MethodCacheMock), nameof(MethodCacheMock.MultiMethod), "multidecimal", typeof(decimal));

            Assert.AreNotEqual(methodInfoIntParam, methodInfoDecimalParam);
        }

        [Test]
        public void RetrieveWithNullType_ThrowsError()
        {
            Assert.Catch<ArgumentNullException>(() => Cache.Retrieve<MethodCacheMock>(null));
        }
    }
}