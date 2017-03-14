using MimeKit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Modules;
using Unosquare.Swan.Networking;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    class Program
    {
        private static int Port = 8080;
        private static string DefaultHttp = "http://localhost:" + Port;

        private const string Authorization = "Authorization";
        private const string AuthorizationToken = "Token";

        static void Main(string[] args)
        {
            Task.Factory.StartNew(async () =>
            {
                using (var webserver = new WebServer(9090))
                {
                    webserver.RegisterModule(new FallbackModule((srv, ctx) =>
                    {
                        ctx.JsonResponse(new Dictionary<string, string>
                        {
                            {Authorization, ctx.RequestHeader(Authorization)}
                        });

                        return true;
                    }));

                    var task = webserver.RunAsync();

                    var data = await JsonClient.Post(DefaultHttp, BasicJson.GetDefault(), AuthorizationToken);

                    Assert.IsNotNull(data);
                    Assert.IsTrue(data.ContainsKey(Authorization));
                    Assert.AreEqual($"Bearer {AuthorizationToken}", data[Authorization]);
                }
            });
            Console.ReadLine();
        }
    }
}