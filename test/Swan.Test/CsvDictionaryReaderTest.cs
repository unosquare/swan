namespace Swan.Test;

using NUnit.Framework;
using Swan.Formatters;
using System.Text;

[TestFixture]
public class CsvDictionaryReaderTest
{
    private const string Data = @"Company,OpenPositions,MainTechnology,Revenue
Co,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 "" 
Ca,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 """;

    private static readonly MemoryStream stream = new(Encoding.ASCII.GetBytes(Data));
    private readonly CsvDictionaryReader dictionaryReader = new(stream);
    private readonly Dictionary<string, string> dict = new()
    {
        { "Company", "Company" },
    };

    [Test]
    public void WithNullValues_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => dictionaryReader.AddMappings(null));
        Assert.Throws<ArgumentNullException>(() => dictionaryReader.AddMapping(null, null));
        Assert.Throws<ArgumentNullException>(() => dictionaryReader.AddMapping("Company", null));
        Assert.Throws<ArgumentException>(() => dictionaryReader.AddMapping("Heading", "Heading"));
    }

    [Test]
    public void WithMapDictionary_AddMapings()
    {
        dict.TryGetValue("Company", out var company);

        dictionaryReader.AddMappings(dict);

        dictionaryReader.Current.TryGetValue("Company", out var addedMap);
        
        Assert.AreEqual(company, addedMap);
    }

    [Test]
    public void WithHeadingMame_RemovesMapings()
    {
        dict.TryGetValue("Company", out var key);

        dictionaryReader.AddMappings(dict);

        var headings = dictionaryReader.RemoveMapping("Company").Current;

        foreach (var kvp in headings)
        {
            Assert.AreNotEqual(kvp.Key, key);
        }
    }
}
