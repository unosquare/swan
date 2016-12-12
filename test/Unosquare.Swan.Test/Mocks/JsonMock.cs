namespace Unosquare.Swan.Test.Mocks
{
    public class BasicJson
    {
        public string StringData { get; set; }

        public int IntData { get; set; }

        public int NegativeInt { get; set; }

        public decimal DecimalData { get; set; }

        public bool BoolData { get; set; }

        public string StringNull { get; set; }
    }

    public class BasicArrayJson
    {
        public int Id { get; set; }

        public string[] Properties { get; set; }
    }
}
