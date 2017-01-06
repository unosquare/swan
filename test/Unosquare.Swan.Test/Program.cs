using System;

namespace Unosquare.Swan.Test
{
    class Program
    {
#if NET452
        static AppDomain otherDomain;
#endif

        static void Main(string[] args)
        {
#if NET452
            otherDomain = AppDomain.CreateDomain("other domain");

            var otherType = typeof(OtherProgram);
            var obj = otherDomain.CreateInstanceAndUnwrap(
                                     otherType.Assembly.FullName,
                                     otherType.FullName) as OtherProgram;
            
            AppDomain.CurrentDomain.FriendlyName.Debug();
            CurrentApp.EntryAssembly.ToString().Debug();
            obj.Main(args);
#endif
            Terminal.ReadKey(true, true);
        }
    }

#if NET452
    public class OtherProgram : MarshalByRefObject
    {
        public void Main(string[] args)
        {
            AppDomain.CurrentDomain.FriendlyName.Debug();
            CurrentApp.EntryAssembly.ToString().Debug();
        }
    }
#endif
}