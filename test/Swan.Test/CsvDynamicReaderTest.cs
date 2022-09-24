namespace Swan.Test;

using NUnit.Framework;
using Swan.Formatters;
using System.Text;

[TestFixture]
public class CsvDynamicReaderTest
{
    protected readonly string Data = @"Company,OpenPositions,MainTechnology,Revenue
Co,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 "" 
Ca,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 """;

    [Test]
    public void WithNullValues_ThrowsException()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        var dynamicReader = new CsvDynamicReader(stream);

        Assert.Throws<ArgumentNullException> (()=> dynamicReader.AddMappings(null));
        Assert.Throws<ArgumentNullException>(() => dynamicReader.AddMapping(null,null));
        Assert.Throws<ArgumentNullException>(() => dynamicReader.AddMapping("Company", null));
        Assert.Throws<ArgumentException>(() => dynamicReader.AddMapping("Heading", "Heading"));
    }

    [Test]
    public void WithMapDictionary_AddMapings()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        var dynamicReader = new CsvDynamicReader(stream);

        var dict = new Dictionary<string, string>()
        {
            { "Company", "Company" },
        };

        dict.TryGetValue("Company", out string company);

        dynamicReader.AddMappings(dict);

        Assert.AreEqual(company, dynamicReader.Current.Company);
    }

    [Test]
    public void WithHeadingMame_RemovesMapings()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        var dynamicReader = new CsvDynamicReader(stream);

        var dict = new Dictionary<string, string>()
        {
            { "Company", "Company" },
        };

        dict.TryGetValue("Company", out string key);

        dynamicReader.AddMappings(dict);

        var headings = dynamicReader.RemoveMapping("Company").Current;

        foreach (var kvp in headings)
        { 
            Assert.AreNotEqual(kvp.Key, key);
        }
     }
}
