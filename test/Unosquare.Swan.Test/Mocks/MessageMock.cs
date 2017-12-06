using System;

namespace Unosquare.Swan.Test.Mocks
{
    public class SimpleMessageMock : Components.MessageHubGenericMessage<string>
    {
        public SimpleMessageMock(object sender, string content = "Unosquare Américas") 
            : base(sender, content)
        {
            // placeholder
        }
    }

    public class SimpleMessageMockCancellable : Components.MessageHubCancellableGenericMessage<string>
    {
        public SimpleMessageMockCancellable(object sender, string content, Action cancelAction)
            : base(sender, content, cancelAction)
        {
            // placeholder
        }
    }
}
