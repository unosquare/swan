namespace Swan.Test.Mocks
{
    public class SimpleMessageMock : Messaging.MessageHubGenericMessage<string>
    {
        public SimpleMessageMock(object sender, string content = nameof(SimpleMessageMock))
            : base(sender, content)
        {
            // placeholder
        }
    }
}