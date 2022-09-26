namespace Swan.Test;

using NUnit.Framework;
using Swan.Formatters;
using System.Text;

[TestFixture]
public class CsvDynamicReaderTest
{
    private const string Data = @"Company,OpenPositions,MainTechnology,Revenue
Co,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 "" 
Ca,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 """;
   
    private static readonly MemoryStream stream = new(Encoding.ASCII.GetBytes(Data));
    private readonly CsvDynamicReader dynamicReader = new(stream);
    private readonly Dictionary<string, string> dict = new()
    {
        { "Company", "Company" },
    };

    [Test]
    public void WithNullValues_ThrowsException()
    {
        Assert.Throws<ArgumentNullException> (()=> dynamicReader.AddMappings(null));
        Assert.Throws<ArgumentNullException>(() => dynamicReader.AddMapping(null,null));
        Assert.Throws<ArgumentNullException>(() => dynamicReader.AddMapping("Company", null));
        Assert.Throws<ArgumentException>(() => dynamicReader.AddMapping("Heading", "Heading"));
    }

    [Test]
    public void WithMapDictionary_AddMapings()
    {
        dict.TryGetValue("Company", out var company);

        dynamicReader.AddMappings(dict);

        Assert.AreEqual(company, dynamicReader.Current.Company);
    }

    [Test]
    public void WithHeadingMame_RemovesMapings()
    {
        dict.TryGetValue("Company", out var key);

        dynamicReader.AddMappings(dict);

        var headings = dynamicReader.RemoveMapping("Company").Current;

        foreach (var kvp in headings)
        { 
            Assert.AreNotEqual(kvp.Key, key);
        }
     }
}
