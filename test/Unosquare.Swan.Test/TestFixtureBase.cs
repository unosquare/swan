namespace Unosquare.Swan.Test
{
    using System;
    using System.Collections.Generic;
    using Mocks;

    public abstract class TestFixtureBase
    {
        protected Dictionary<object, object> NullDict = null;

        protected object NullObj = null;

        protected List<string> NullStringList = null;

        protected Action NullAction = null;

        protected DateBasicJson DefaultObject = new DateBasicJson
        {
            StringData = "string",
            IntData = 1,
            NegativeInt = -1,
            DecimalData = 10.33M,
            BoolData = true,
            Date = new DateTime(2017, 10, 10)
        };

        protected SampleStruct DefaultStruct = new SampleStruct
        {
            Name = "string",
            Value = 1
        };
    }
}
