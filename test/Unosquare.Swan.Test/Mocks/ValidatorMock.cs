namespace Unosquare.Swan.Test.Mocks
{
    using Unosquare.Swan.Lite.Attributes;

    public class SimpleValidationMock
    {
        [Email]
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class AttributeValidatorMock
    {
        [NotNull]
        [Range(1, 10)]
        public int Age { get; set; }

        [Range(1.2, 5.5)]
        public string Temperature { get; set; }
    }
}
