namespace Unosquare.Swan.Components
{
    using System;

    public partial class ObjectMapper
    {
        internal class TypeValuePair
        {
            public TypeValuePair(Type type, object value)
            {
                Type = type;
                Value = value;
            }

            public Type Type { get; }

            public object Value { get; }
        }
    }
}