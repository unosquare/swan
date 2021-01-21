namespace Swan.Test.Mocks
{
    using System;
    using Formatters;
    using Mappers;

    public class ErrorJson
    {
        public string Message { get; set; }
    }

    public class BasicJsonWithoutCtor
    {
        protected BasicJsonWithoutCtor()
        {
            // Ignore
        }
    }

    public class BasicJson
    {
        public string StringData { get; set; }

        public int IntData { get; set; }

        public int NegativeInt { get; set; }

        public decimal DecimalData { get; set; }

        public bool BoolData { get; set; }

        public string? StringNull { get; set; }

        public static BasicJson GetDefault() =>
            new()
            {
                StringData = "string,\r\ndata\\",
                IntData = 1,
                NegativeInt = -1,
                DecimalData = 10.33M,
                BoolData = true,
            };

        public static string GetControlValue() =>
            "\"StringData\": \"string,\\r\\ndata\\\\\",\"IntData\": 1,\"NegativeInt\": -1,\"DecimalData\": 10.33,\"BoolData\": true,\"StringNull\": null";
    }

    public class BasicJsonWithNewProperty : BasicJson
    {
        public new int StringNull { get; set; }
    }

    public class DateBasicJson : BasicJson
    {
        public DateTime Date { get; set; }
    }

    public class EmptyJson
    {
        // Nothing
    }

    public class BasicArrayJson
    {
        public int Id { get; set; }

        public string[] Properties { get; set; }
    }

    public class AdvJson : BasicJson
    {
        public BasicJson InnerChild { get; set; }
    }

    public class AdvArrayJson
    {
        public int Id { get; set; }

        public BasicJson[] Properties { get; set; }
    }

    public class ArrayJsonWithInitialData
    {
        public int Id { get; set; } = 1;

        public string[] Properties { get; set; } = { "ONE", "TWO" };
    }

    public class DateTimeJson
    {
        public DateTime? Date { get; set; }
    }

    public class JsonPropertySample
    {
        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("ignoredData", true)]
        public string IgnoredData { get; set; }
    }

    public class InnerJsonPropertySample
    {
        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("ignoredData", true)]
        public string IgnoredData { get; set; }

        public JsonPropertySample Inner { get; set; }
    }

    public class JsonIngoreNestedPropertySample
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data")]
        public int Data { get; set; }
    }

    public class JsonIngorePropertySample
    {
        [JsonProperty("id", true)]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("inner")]
        public JsonIngoreNestedPropertySample Inner { get; set; }
    }

    public class JsonFile
    {
        public string Filename { get; set; }
        public byte[] Data { get; set; }
    }

    public struct SampleStruct
    {
        public int Value;
        public string Name;
    }

    public struct SampleStructWithProps
    {
        public int StudentId { get; set; }

        public double Average { get; set; }

        public string Notes { get; set; }
    }

    public class ObjectEnum
    {
        public int Id { get; set; }
        public MyEnum MyEnum { get; set; } = MyEnum.Three;
    }

    public class ObjectNoEmptyCtor
    {
        public ObjectNoEmptyCtor(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }

    public enum MyEnum
    {
        One = 1,
        Two,
        Three,
    }

    [Flags]
    public enum MyFlag
    {
        NoneOrZero = 0,
        One = 1,
        Two = 2,
        All = One | Two,
    }

    [Flags]
    public enum MyFlagByte : byte
    {
        NoneOrZero = 0,
        One = 1,
        Two = 2,
        All = One | Two,
    }

    [Flags]
    public enum MyFlagLong : long
    {
        NoneOrZero = 0,
        One = 1,
        Two = 2,
        All = One | Two,
    }

    [Flags]
    public enum MyFlag2
    {
        None = 0,
        One = 1,
        Two = 2,
    }

    public class ObjectAttr
    {
        public int Id { get; set; }

        [Copyable]
        public string Name { get; set; }

        [Copyable]
        public bool IsActive { get; set; }

        public string Owner { get; set; }

        public static ObjectAttr GetDefault() =>
            new()
            {
                Id = 1,
                IsActive = true,
                Name = "swan",
                Owner = "UnoLabs",
            };
    }

    public class ObjectWithArray
    {
        public string[] Data { get; set; }
    }
}
