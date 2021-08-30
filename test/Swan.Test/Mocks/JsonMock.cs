using Swan.Mappers;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Swan.Test.Mocks
{
    public class ErrorJson
    {
        public string Message { get; set; }
    }

    public class SamplePerson
    {
        private string m_Specialty = "Content of private field.";

        public Guid Id { get; set; }

        public string? Name { get; set; }

        public int Age { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string? GreetingField;

        public int? CarModelYear { get; set; }

        public SamplePerson? RelatedPerson { get; set; }

        public string Specialty => m_Specialty;

        public List<int> Primes { get; set; } = new List<int>() { 2, 3, 5, 7, 11 };
    }

    public class SampleFamily
    {
        public Dictionary<string, SamplePerson> Members { get; set; } = new Dictionary<string, SamplePerson>();

        public static SampleFamily Create(bool createCycle)
        {
            var result = new SampleFamily();
            result.Members.Add("Dad", new()
            {
                Age = 32,
                CarModelYear = 2009,
                DateOfBirth = new(2009, 09, 05),
                GreetingField = "Hello, I'm the dad",
                Id = Guid.NewGuid(),
                Name = "Unosquare"
            });

            result.Members.Add("Mom", new()
            {
                Age = 18,
                CarModelYear = 2014,
                DateOfBirth = new(2014, 09, 05),
                GreetingField = "Hello, I'm the mom",
                Id = Guid.NewGuid(),
                Name = "Labs"
            });

            if (createCycle)
            {
                result.Members["Dad"].RelatedPerson = result.Members["Mom"];
                result.Members["Mom"].RelatedPerson = result.Members["Dad"];
            }
                

            return result;
        }
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
            "\"StringData\":\"string,\\r\\ndata\\\\\",\"IntData\":1,\"NegativeInt\":-1,\"DecimalData\":10.33,\"BoolData\":true,\"StringNull\":null";
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
        [JsonPropertyName("data")]
        public string Data { get; set; }

        [JsonPropertyName("ignoredData")]
        [JsonIgnore]
        public string IgnoredData { get; set; }
    }

    public class InnerJsonPropertySample
    {
        [JsonPropertyName("data")]
        public string Data { get; set; }

        [JsonPropertyName("ignoredData")]
        [JsonIgnore]
        public string IgnoredData { get; set; }

        public JsonPropertySample Inner { get; set; }
    }

    public class JsonIngoreNestedPropertySample
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("data")]
        public int Data { get; set; }
    }

    public class JsonIngorePropertySample
    {
        [JsonPropertyName("id")]
        [JsonIgnore]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("inner")]
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