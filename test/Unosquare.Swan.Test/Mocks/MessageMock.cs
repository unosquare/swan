namespace Unosquare.Swan.Test.Mocks
{
    public class SimpleMessageMock : Swan.Components.MessageHubGenericMessage<string>
    {
        public SimpleMessageMock(object sender, string content) : base(sender, content)
        {
            // placeholder
        }
    }
}
