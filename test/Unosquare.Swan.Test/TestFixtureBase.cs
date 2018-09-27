namespace Unosquare.Swan.Test
{
    using System;
    using System.Collections.Generic;
    using Mocks;

    public abstract class TestFixtureBase
    {
        protected Dictionary<object, object> NullDict => null;

        protected object NullObj => null;

        protected string NullString => null;

        protected byte[] NullByteArray => null;

        protected List<string> NullStringList => null;

        protected Type NullType => null;

        protected Action NullAction => null;

        protected Exception NullException => null;

        protected DateBasicJson DefaultObject => new DateBasicJson
        {
            StringData = "string",
            IntData = 1,
            NegativeInt = -1,
            DecimalData = 10.33M,
            BoolData = true,
            Date = new DateTime(2017, 10, 10),
        };

        protected SampleStruct DefaultStruct => new SampleStruct
        {
            Name = nameof(DefaultStruct),
            Value = 1,
        };

        protected List<string> DefaultStringList => new List<string> { "A", "B", "C" };

        protected Dictionary<int, string> DefaultDictionary => new Dictionary<int, string>
        {
            {1, "A"},
            {2, "B"},
            {3, "C"},
            {4, "D"},
            {5, "E"},
        };
    }
}
