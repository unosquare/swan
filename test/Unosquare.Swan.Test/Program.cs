using MimeKit;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Unosquare.Swan.Networking;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var mock = new AppWorkerMock();
            {
                var exit = false;
                mock.OnExit = () => exit = true;
                Assert.AreEqual(AppWorkerState.Stopped, mock.State);
                mock.Start();
                Assert.IsTrue(mock.IsBusy);
                Assert.AreEqual(AppWorkerState.Running, mock.State);
                mock.Stop();
                Assert.AreEqual(AppWorkerState.Stopped, mock.State);

                Assert.IsTrue(mock.ExitBecauseCancellation, "Exit because cancellation");
                Assert.IsTrue(exit, "Exit event was fired");
            }

            Terminal.ReadKey(true, true);
        }
    }
}