namespace Swan.Test;

using NUnit.Framework;
using Formatters;
using Mocks;
using System.Dynamic;
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

        Assert.AreEqual("Merlina", jsonObject?.Name);
    }

    [Test]
    public async Task WithNullStream_ReturnsNull()
    {
        var jsonStream = null as Stream;
        var jsonObject = await jsonStream.JsonDeserializeAsync();

        Assert.IsNull(jsonObject);
    }

    [Test]
    public async Task WithUTF8JsonStream_ReturnsDynamicObject()
    {
        var dog = new Dog { Name = "Merlina" };
        var jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(dog);

        var stream = new MemoryStream(jsonUtf8Bytes);

        var deserializedDog = await stream.JsonDeserializeAsync();
        
        Assert.AreEqual(typeof(ExpandoObject), deserializedDog.GetType());
    }
}
