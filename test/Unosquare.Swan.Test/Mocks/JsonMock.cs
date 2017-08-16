﻿using System;

namespace Unosquare.Swan.Test.Mocks
{
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
                StringData = "string",
                IntData = 1,
                NegativeInt = -1,
                DecimalData = 10.33M,
                BoolData = true
            };
        }
    }

    public class DateBasicJson : BasicJson
    {
        public DateTime Date { get; set; }

        public static DateBasicJson GetDateDefault()
        {
            return new DateBasicJson
            {
                StringData = "string",
                IntData = 1,
                NegativeInt = -1,
                DecimalData = 10.33M,
                BoolData = true,
                Date = new DateTime(2017, 10, 10)
            };
        }
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

    public class ObjectEnum
    {
        private MyEnum _value;

        public int Id { get; set; }
        public MyEnum MyEnum
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }
    }

    public enum MyEnum
    {
        One = 1,
        Two,
        Three
    }
}
