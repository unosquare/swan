namespace Swan.Test.Mocks;

internal class MethodCacheMock
{
    public static Task<T> GetMethodTest<T>(string value) => Task.FromResult(default(T));
}
