namespace Swan.Test;

using Formatters;
using Mocks;
using System.Text.Json;

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

    [Test]
    public void WithDictionary_ReturnsRepresentation()
    {
        Dictionary<int, string> dict = new Dictionary<int, string>
        {
            { 1, "One" }, 
            { 2, "Two" },
            { 3, "Three" }
        };

        var serialized = TextSerializer.Serialize(dict);

        Assert.AreEqual("{\r\n    \"1\": \"One\",\r\n    \"2\": \"Two\",\r\n    \"3\": \"Three\"\r\n}", serialized);
    }

    [Test]
    public void WithArray_ReturnsRepresentation()
    {
        int[] intARray = new int[]
        {
            1,2,3
        };

        var serialized = TextSerializer.Serialize(intARray);

        Assert.AreEqual("[1, 2, 3]", serialized);
    }

    [Test]
    public void WithJson_ReturnsRepresentation()
    {
        JsonDocument jd = JsonDocument.Parse("{\"CarModel\":\"Vocho\"}");
        JsonElement je = jd.RootElement;
        
        var serialized = TextSerializer.Serialize(je);

        Assert.AreEqual("{\r\n    \"CarModel\": \"Vocho\"\r\n}", serialized);
    }
}
