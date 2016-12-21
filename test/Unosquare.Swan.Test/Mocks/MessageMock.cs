using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Unosquare.Swan.Test.Mocks
{
    internal class SimpleMessageMock : Unosquare.Swan.Runtime.MessageHubGenericMessage<string>
    {
        public SimpleMessageMock(object sender, string content) : base(sender, content)
        {
            // placeholder
        }
    }
}
