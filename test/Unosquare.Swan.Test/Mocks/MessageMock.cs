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
}
