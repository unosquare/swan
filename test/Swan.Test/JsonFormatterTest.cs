namespace Swan.Test;

using NUnit.Framework;
using Formatters;
using Mocks;
using System.Text.Json;

[TestFixture]
public class JsonFormatterTest
{
    [Test]
    public void WithJson_ReturnsDogObject()
    {
        var deserialized = "{\r\n    \"Name\": \"Merlina\"\r\n}".JsonDeserialize(typeof(Dog)) as Dog;
        Assert.AreEqual("Merlina", deserialized?.Name);
    }

    [Test]
    public void WithNullType_ThrowsException()
    {
        Stream stream = null;
        Assert.Throws<ArgumentNullException>(() => "{\r\n    \"Name\": \"Merlina\"\r\n}".JsonDeserialize(null));
        Assert.ThrowsAsync<ArgumentNullException>(() => stream.JsonDeserializeAsync(null));
    }

    [Test]
    public async Task WithUTF8JsonStream_ReturnsObject()
    {
        var dog = new Dog { Name = "Merlina" };
        byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(dog);

        var stream = new MemoryStream(jsonUtf8Bytes);

        var deserializedDog = await stream.JsonDeserializeAsync(typeof(Dog)) as Dog;

        Assert.AreEqual(dog.Name, deserializedDog?.Name);
    }

    [Test]
    public async Task WithUTF8JsonStream_ReturnsDinamicObject()
    {
        var dog = new Dog { Name = "Merlina" };
        byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(dog);

        var stream = new MemoryStream(jsonUtf8Bytes);

        var deserializedDog = await stream.JsonDeserializeAsync<Dog>();

        Assert.AreEqual(dog.Name, deserializedDog?.Name);
    }
}
