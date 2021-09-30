namespace Swan.Test
{
    using Swan.Test.Mocks;
    using System;
    using System.Collections.Generic;

    public abstract class TestFixtureBase
    {
        protected Dictionary<object, object>? NullDict => null;

        protected object? NullObj => null;

        protected string? NullString => null;

        protected byte[]? NullByteArray => null;

        protected List<string>? NullStringList => null;

        protected Type? NullType => null;

        protected Action? NullAction => null;

        protected Exception? NullException => null;

        protected DateBasicJson DefaultObject => new()
        {
            StringData = "string",
            IntData = 1,
            NegativeInt = -1,
            DecimalData = 10.33M,
            BoolData = true,
            Date = new DateTime(2017, 10, 10),
        };

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
}