namespace Unosquare.Swan.Test.Mocks
{
    internal class SimpleMessageMock : Unosquare.Swan.Components.MessageHubGenericMessage<string>
    {
        public SimpleMessageMock(object sender, string content) : base(sender, content)
        {
            // placeholder
        }
    }
}
