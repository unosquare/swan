namespace Unosquare.Swan.Test.Mocks
{
    using System.Threading.Tasks;

    internal class MethodCacheMock
    {
        public static Task<T> GetMethodTest<T>(string value) => Task.FromResult(default(T));

        public void SingleMethod()
        {
            // do nothing
        }

        public void MultiMethod(int number)
        {
            // do nothing
        }

        public void MultiMethod(decimal number)
        {
            // do nothing
        }

        public void AmbiguousMethod(int number)
        {
            // do nothing
        }

        public void AmbiguousMethod(decimal number)
        {
            // do nothing
        }
    }
}
