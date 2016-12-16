using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.Swan.Abstractions;

namespace Unosquare.Swan.Test.Mocks
{
    internal class MockProvider : SingletonBase<MockProvider>
    {
        internal string GetName() => nameof(MockProvider);
    }
}
