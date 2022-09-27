namespace Swan.Test;

using Swan.Formatters;
using Swan.Test.Mocks;

[TestFixture]
public class CsvWriterGenericTest
{
    private readonly Func<dynamic, string> ValueProvider = str => str.ToUpper();

    [Test]
    public void WithNullValuesGiven_ThrowsException()
    {
        var stream = new MemoryStream();
        var writer = new CsvWriter<dynamic>(stream);

        Assert.Throws<ArgumentNullException>(() => writer.WriteLine(null));
        Assert.Throws<ArgumentNullException>(() => writer.AddMapping(null, ValueProvider));
        Assert.Throws<ArgumentNullException>(() => writer.RemoveMapping(null));
    }

    [Test]
    public void AddMapingWhenHasWrittenHeadingsTrue_ThrowsExepction()
    {
        var objHeaders = new SampleCsvRecord();

        using var stream = new MemoryStream();
        using var writer = new CsvWriter<SampleCsvRecord>(stream, separatorChar: '#');

        writer.WriteLine(objHeaders);
        writer.Flush();

        Assert.Throws<InvalidOperationException>(() => writer.AddMapping("HeaderName", ValueProvider));
    }

    [Test]
    public void ClearMapingsWhenHasWrittenHeadingsTrue_ThrowsExepction()
    {
        var objHeaders = new SampleCsvRecord();

        using var stream = new MemoryStream();
        using var writer = new CsvWriter<SampleCsvRecord>(stream, separatorChar: '#');

        writer.WriteLine(objHeaders);
        writer.Flush();

        Assert.Throws<InvalidOperationException>(() => writer.ClearMappings());
    }

    [Test]
    public void RemoveMapingsWhenHasWrittenHeadingsTrue_ThrowsExepction()
    {
        var objHeaders = new SampleCsvRecord();

        using var stream = new MemoryStream();
        using var writer = new CsvWriter<SampleCsvRecord>(stream, separatorChar: '#');

        writer.WriteLine(objHeaders);
        writer.Flush();

        Assert.Throws<InvalidOperationException>(() => writer.RemoveMapping("HeadingName"));
    }

    [Test]
    public void WithCSVWriter_addsMapping()
    {
        using var stream = new MemoryStream();
        using var writer = new CsvWriter<dynamic>(stream);

        writer.AddMapping("HeadingName", ValueProvider);

        Assert.IsNotNull(writer.PropertyMap.ContainsKey("HeadingName"));
    }

    [Test]
    public void WithCSVWriter_addsAndThenClearsMappings()
    {
        using var stream = new MemoryStream();
        using var writer = new CsvWriter<dynamic>(stream);

        writer.AddMapping("HeadingName", ValueProvider);
        writer.ClearMappings();

        Assert.AreEqual(0, writer.PropertyMap.Count);
    }

    [Test]
    public void WithCSVWriter_addsAndThenRemovesMappingByName()
    {
        using var stream = new MemoryStream();
        using var writer = new CsvWriter<dynamic>(stream);

        writer.AddMapping("HeadingName", ValueProvider);
        writer.RemoveMapping("HeadingName");

        Assert.AreEqual(0, writer.PropertyMap.Count);
    }
}
