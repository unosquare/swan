namespace Swan.Test;

using NUnit.Framework;
using Swan.Formatters;
using System.Text;

[TestFixture]
public class CsvDictionaryReaderTest
{
    protected readonly string Data = @"Company,OpenPositions,MainTechnology,Revenue
Co,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 "" 
Ca,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 """;

    [Test]
    public void WithNullValues_ThrowsException()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        var dictionaryReader = new CsvDictionaryReader(stream);

        Assert.Throws<ArgumentNullException>(() => dictionaryReader.AddMappings(null));
        Assert.Throws<ArgumentNullException>(() => dictionaryReader.AddMapping(null, null));
        Assert.Throws<ArgumentNullException>(() => dictionaryReader.AddMapping("Company", null));
        Assert.Throws<ArgumentException>(() => dictionaryReader.AddMapping("Heading", "Heading"));
    }

    [Test]
    public void WithMapDictionary_AddMapings()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        var dictionaryReader = new CsvDictionaryReader(stream);

        var dict = new Dictionary<string, string>()
        {
            { "Company", "Company" },
        };

        dict.TryGetValue("Company", out string company);

        dictionaryReader.AddMappings(dict);

        dictionaryReader.Current.TryGetValue("Company", out string addedMap);
        
        Assert.AreEqual(company, addedMap);
    }

    [Test]
    public void WithHeadingMame_RemovesMapings()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        var dictionaryReader = new CsvDictionaryReader(stream);

        var dict = new Dictionary<string, string>()
        {
            { "Company", "Company" },
        };

        dict.TryGetValue("Company", out string key);

        dictionaryReader.AddMappings(dict);

        var headings = dictionaryReader.RemoveMapping("Company").Current;

        foreach (var kvp in headings)
        {
            Assert.AreNotEqual(kvp.Key, key);
        }
    }
}
