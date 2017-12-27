namespace Unosquare.Swan.Test.Mocks
{
    using System;

    public class SimpleMessageMock : Components.MessageHubGenericMessage<string>
    {
        public SimpleMessageMock(object sender, string content = "Unosquare Américas") 
            : base(sender, content)
        {
            // placeholder
        }
    }
}
