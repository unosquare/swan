using Unosquare.Swan.Runtime;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var options = new OptionMock();
            var collection = new[] { 10, 30, 50 };

            var dumpArgs = new[] { "--options", string.Join(",", collection) };
            
            if (CmdArgsParser.Default.ParseArguments(dumpArgs, options))
                "OK".Info();

            Terminal.ReadKey(true);
        }
    }
}