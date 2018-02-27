namespace Unosquare.Swan.Test.Mocks
{
    using Unosquare.Swan.Lite.Attributes;

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
}
