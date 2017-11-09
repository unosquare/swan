namespace Unosquare.Swan.Test.Mocks
{
    using System;
    using Attributes;

    public class ErrorJson
    {
        public string Message { get; set; }
    }

    public class BasicJson
    {
        public string StringData { get; set; }

        public int IntData { get; set; }

        public int NegativeInt { get; set; }

        public decimal DecimalData { get; set; }

        public bool BoolData { get; set; }

        public string StringNull { get; set; }

        public static BasicJson GetDefault()
        {
            return new BasicJson
            {
                StringData = "string,\r\ndata",
                IntData = 1,
                NegativeInt = -1,
                DecimalData = 10.33M,
                BoolData = true
            };
        }

        public static string GetControlValue() =>
            "\"StringData\": \"string,\\r\\ndata\",\"IntData\": 1,\"NegativeInt\": -1,\"DecimalData\": 10.33,\"BoolData\": true,\"StringNull\": null";
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

        public string[] Properties { get; set; } = new[] { "ONE", "TWO" };
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
        public MyEnum MyEnum{ get; set; }
    }

    public enum MyEnum
    {
        One = 1,
        Two,
        Three
    }

    public class ObjectAttr
    {
        public int Id { get; set; }

        [Copyable]
        public string Name { get; set; }

        [Copyable]
        public bool IsActive { get; set; }

        public string Owner { get; set; }

        public static ObjectAttr Get()
        {
            return new ObjectAttr
            {
                Id = 1,
                IsActive = true,
                Name = "swan",
                Owner = "UnoLabs"
            };
        }
    }
}
