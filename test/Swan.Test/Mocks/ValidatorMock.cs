using Swan.Validators;
using System;

namespace Swan.Test.Mocks
{
    public class SimpleValidationMock
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class NotNullMock
    {
        [NotNull]
        public int? Number { get; set; }
    }

    public class RangeMock
    {
        [Range(1, 10)]
        public int Age { get; set; }

        [Range(0.2, 1)]
        public double Kilograms { get; set; }
    }

    public class InvalidRangeMock
    {
        [Range(1, 10)]
        public string Invalid { get; set; }
    }

    public class RegexMock
    {
        [Match(@"hi|hello")]
        public string Salute { get; set; }

        internal static object TypeInfo()
        {
            throw new NotImplementedException();
        }
    }

    public class InvalidRegexMock
    {
        [Match(@"hi|hello")]
        public int Salute { get; set; }
    }

    public class EmailMock
    {
        [Email]
        public string? To { get; set; }
    }
}