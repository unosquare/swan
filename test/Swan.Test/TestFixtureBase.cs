namespace Swan.Test;

using Mocks;

public abstract class TestFixtureBase
{
    protected Dictionary<object, object>? NullDict => null;

    protected object? NullObj => null;

    protected string? NullString => null;

    protected List<string>? NullStringList => null;

    protected Type? NullType => null;

    protected Exception? NullException => null;

    protected SampleStruct DefaultStruct => new()
    {
        Name = nameof(DefaultStruct),
        Value = 1,
    };

    protected List<string> DefaultStringList => new() { "A", "B", "C" };

    protected Dictionary<int, string> DefaultDictionary => new()
    {
        { 1, "A" },
        { 2, "B" },
        { 3, "C" },
        { 4, "D" },
        { 5, "E" },
    };
}
