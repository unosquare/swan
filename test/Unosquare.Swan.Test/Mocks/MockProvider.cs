namespace Unosquare.Swan.Test.Mocks
{
    using Abstractions;

    internal class MockProvider : SingletonBase<MockProvider>
    {
        internal string GetName() => nameof(MockProvider);
    }
}
