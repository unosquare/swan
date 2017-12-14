namespace Unosquare.Swan.Test.Mocks
{
    using Abstractions;

    internal class MockProvider : SingletonBase<MockProvider>
    {
        internal string GetName() => nameof(MockProvider);
    }

    internal class NumberFactory
    {
        public T GetNumber<T>() => default(T);
    }

    internal class BetterNumberFactory : NumberFactory
    {
        public new T GetNumber<T>() => default(T);
    }
}
