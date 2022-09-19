namespace Swan.Test;

using Formatters;
using Mocks;

[TestFixture]
public class StringSerialize
{
    [Test]
    public void WithString_ReturnsString() => Assert.AreEqual(@"""Hola""", TextSerializer.Serialize("Hola"));
    
    [Test]
    public void WithNumber_ReturnsNumberRepresentation() => Assert.AreEqual(@"1", TextSerializer.Serialize(1));

    [Test]
    public void WithBoolean_ReturnsBoolRepresentation() => Assert.AreEqual("true", TextSerializer.Serialize(true));

    [Test]
    public void WithObject_ReturnsObjectRepresentation() => Assert.AreEqual("{\r\n"+@"    ""Message"": null"+"\r\n}",
        TextSerializer.Serialize(new ErrorJson()));
}
