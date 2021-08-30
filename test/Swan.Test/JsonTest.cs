using Swan.Test.Mocks;

namespace Swan.Test.JsonTests
{
    public abstract class JsonTest : TestFixtureBase
    {
        protected const string ArrayStruct = "[{\"Value\":1,\"Name\":\"A\"},{\"Value\":2,\"Name\":\"B\"}]";

        protected static readonly AdvJson AdvObj = new()
        {
            StringData = "string,\r\ndata\\",
            IntData = 1,
            NegativeInt = -1,
            DecimalData = 10.33M,
            BoolData = true,
            InnerChild = BasicJson.GetDefault(),
        };

        protected static string BasicStr => "{" + BasicJson.GetControlValue() + "}";

        protected string AdvStr =>
            "{\"InnerChild\":" + BasicStr + "," + BasicJson.GetControlValue() + "}";

        protected string BasicAStr => "[\"A\",\"B\",\"C\"]";

        protected int[] NumericArray => new[] { 1, 2, 3 };

        protected string NumericAStr => "[1,2,3]";

        protected BasicArrayJson BasicAObj => new()
        {
            Id = 1,
            Properties = new[] { "One", "Two", "Babu" },
        };

        protected AdvArrayJson AdvAObj => new()
        {
            Id = 1,
            Properties = new[] { BasicJson.GetDefault(), BasicJson.GetDefault() },
        };

        protected string BasicAObjStr => "{\"Id\":1,\"Properties\":[\"One\",\"Two\",\"Babu\"]}";

        protected string AdvAStr => "{\"Id\":1,\"Properties\":[" + BasicStr + "," + BasicStr + "]}";
    }
}