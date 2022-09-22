namespace Swan.Test;

using NUnit.Framework;
using Swan.Formatters;
using Swan.Test.Mocks;
using System.Text.Json;

[TestFixture]
public class JsonFormatterDynamicTest
{
    [Test]
    public void WithJsonTextNullOrWitheSpace_ReturnsNull()
    {
        var jsonText = string.Empty;
        var jsonObject = jsonText.JsonDeserialize();

        Assert.IsNull(jsonObject);
    }

    [Test]
    public void WithJsonText_ReturnsObject()
    {
        var jsonText = "{\r\n    \"Name\": \"Merlina\"\r\n}";
        var jsonObject = jsonText.JsonDeserialize();

        Assert.AreEqual("Merlina", jsonObject.Name);
    }

    [Test]
    public async Task WithNullStream_ReturnsObject()
    {
        Stream jsonStream = null;
        var jsonObject = await jsonStream.JsonDeserializeAsync();

        Assert.IsNull(jsonObject);
    }

    [Test]
    public async Task WithUTF8JsonStream_ReturnsDinamicObject()
    {
        var dog = new Dog() { Name = "Merlina" };
        byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(dog);

        var json = JsonSerializer.Serialize(dog);

        var stream = new MemoryStream(jsonUtf8Bytes);

        var deserializedDog = await stream.JsonDeserializeAsync();

        Assert.AreEqual(dog.Name, deserializedDog.Name);
    }
}
