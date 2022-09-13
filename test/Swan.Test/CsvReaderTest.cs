namespace Swan.Test;

using Formatters;
using Mocks;
using Platform;
using System.Text;

[TestFixture]
public abstract class CsvReaderTest
{
    protected readonly string[] Headers = { "Company", "OpenPositions", "MainTechnology", "Revenue" };

    protected readonly string Data = @"Company,OpenPositions,MainTechnology,Revenue
Co,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 "" 
Ca,2,""C#, MySQL, JavaScript, HTML5 and CSS3"","" $1,359,885 """;

    protected readonly Dictionary<string, string> Map = new()
    {
        { "Company", "Warsong Clan" },
        { "OpenPositions", "Wolfrider" },
        { "MainTechnology", "Axe" },
        { "Revenue", "$190000G" },
    };
}

public class CsvReaderConstructor : CsvReaderTest
{
    [Test]
    public void WithValidStreamAndValidEncoding_ReturnsReader()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        var reader = new CsvReader(stream, Encoding.ASCII);

        Assert.IsNotNull(reader);
    }

    [Test]
    public void WithNullStream_ThrowsNullReferenceException() =>
        Assert.Throws<ArgumentNullException>(() =>
        {
            var _ = new CsvReader(default(MemoryStream), Encoding.ASCII);
        });

    [Test]
    public void WithNullEncoding_ThrowsNullReferenceException() =>
        Assert.Throws<ArgumentNullException>(() =>
        {
            var _ = new CsvReader(default(MemoryStream));
        });
}

public class SkipRecord : CsvReaderTest
{
    [Test]
    public void WithValidStream_SkipsRecord()
    {
        const int position = 0;

        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        var reader = new CsvReader(stream, Encoding.ASCII);
        reader.Skip();
        Assert.AreNotEqual(stream.Position, position);
    }

    [Test]
    public void WithValidStringAndEscapeCharacter_SkipsRecord()
    {
        const int position = 0;
        const string data = "Orgrimmar,m";

        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(data));
        var reader = new CsvReader(stream, Encoding.ASCII, escapeChar: 'm');

        reader.Skip();

        Assert.AreNotEqual(stream.Position, position);
    }

    [Test]
    public void WithInvalidString_ThrowsEndOfStreamException()
    {
        var tempFile = Path.GetTempFileName();
        using var fs = File.OpenRead(tempFile);
        using var reader = new CsvReader(fs);
        Assert.Throws<EndOfStreamException>(() => reader.Skip());
    }
}

public class ReadHeadings : CsvReaderTest
{
    [Test]
    public void WithValidStream_ReturnsAnArray()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        using var reader = new CsvDictionaryReader(stream);
            

        var headings = reader.Headings.Keys.ToArray();
        Assert.IsNotEmpty(headings);
        Assert.AreEqual(Headers, headings);
    }

    [Test]
    public void WithReadHeadingsAlreadyCalled_ThrowsInvalidOperationException()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        using var reader = new CsvDictionaryReader(stream);
        var item = reader.Current;

        Assert.Throws<InvalidOperationException>(() => reader.SetHeadings("x", "y"));
    }

    [Test]
    public void WithReadHeadingsAsSecondOperation_ThrowsInvalidOperationException()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        using var reader = new CsvDictionaryReader(stream);
        var item = reader.Current;

        Assert.Throws<InvalidOperationException>(() => reader.SetHeadings());
    }
}

public class ReadLine : CsvReaderTest
{
    [Test]
    public void WithValidStream_ReturnsAnArray()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        var reader = new CsvReader(stream);
        reader.MoveNext();
            
        Assert.IsNotEmpty(reader.Values);
    }

    [Test]
    public void WithInvalidString_ThrowsEndOfStreamException()
    {
        var tempFile = Path.GetTempFileName();
        using var fs = File.OpenRead(tempFile);
        using var reader = new CsvReader(fs);
        Assert.Throws<EndOfStreamException>(() => reader.Skip());
    }

    [Test]
    public void WithInvalidStringAndEncoding_ThrowsEndOfStreamException()
    {
        var tempFile = Path.GetTempFileName();
        using var fs = File.OpenRead(tempFile);
        using var reader = new CsvReader(fs, SwanRuntime.Windows1252Encoding);
        Assert.Throws<EndOfStreamException>(() => reader.Skip());
    }
}

public class ReadObject : CsvReaderTest
{
    [Test]
    public void WithValidStream_ReturnsADictionary()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        using var reader = new CsvDictionaryReader(stream);
        reader.MoveNext();
        Assert.IsNotNull(reader.Current);
    }

    [Test]
    public void WithoutReadHeadingsCall_ThrowsInvalidOperationException()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        using var reader = new CsvDictionaryReader(stream);

        Assert.Throws<InvalidOperationException>(() =>
        {
            reader.SetHeadings("").SetHeadings().MoveNext();
        });
    }

    [Test]
    public void WithNullAsParam_ThrowsArgumentNullException()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        using var reader = new CsvDictionaryReader(stream);
        Assert.IsNull(reader.Values);

        Assert.IsNotNull(reader.Current);
        Assert.Catch(() => reader.SetHeadings(null));
    }

    [Test]
    public void WithSampleDto_ThrowsArgumentNullException()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        using var reader = new CsvObjectReader<UserDto>(stream);
        var result = new List<UserDto>();

        foreach (var item in reader)
        {
            reader.Skip();
            result.Add(item);
        }

        Assert.IsTrue(reader.Count == 2);
        Assert.IsTrue(result.Count == 1);
    }

    [Test]
    public void WithInvalidTempFile_ThrowsEndOfStreamException()
    {
        var tempFile = Path.GetTempFileName();
        using var fs = File.OpenRead(tempFile);
        using var reader = new CsvObjectReader<UserDto>(fs);

        Assert.Throws<InvalidOperationException>(() =>
        {
            var c = reader.Current;
        });
    }

    [Test]
    public void WithNoReadHeadingsCall_ThrowsInvalidOperationException()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        var reader = new CsvObjectReader<UserDto>(stream);
        reader.AddMapping("Company", dto => dto.Name);
        reader.AddMapping("OpenPositions",
            dto => dto.StartDate, 
            s => new(2000 + int.Parse(s), 4,6));

        var result = new List<UserDto>(reader);
        Assert.IsTrue(result.Count == 2);
    }
}

public class Count : CsvReaderTest
{
    [Test]
    public void WithValidStream_ReturnsReadCount()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        using var reader = new CsvDictionaryReader(stream);
        _ = reader.First();
        Assert.AreEqual(2, reader.Count);
    }
}

public class EscapeCharacter : CsvReaderTest
{
    [Test]
    public void WithValidStream_GetsAndSetsSeparatorEscapeCharacter()
    {
        var reader = new CsvReader(new MemoryStream(), escapeChar: '?');

        Assert.AreEqual('?', reader.EscapeChar);
    }
}

public class SeparatorCharacter : CsvReaderTest
{
    [Test]
    public void WithValidStream_GetsAndSetsSeparatorCharacter()
    {
        using var reader = new CsvReader(new MemoryStream(), separatorChar: '+');

        Assert.AreEqual('+', reader.SeparatorChar);
    }
}

public class Dispose : CsvReaderTest
{
    [Test]
    public void WithDisposeAlreadyCalled_SetsHasDisposeToTrue()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(Data));
        var reader = new CsvDictionaryReader(stream);
        var readObj = reader.Current;
        reader.Dispose();
        reader.Dispose();

        Assert.IsNotNull(readObj);
    }
}
