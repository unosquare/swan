using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Modules;
using Unosquare.Swan.Networking;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class JsonClientTest
    {
        private static int Port = 8080;
        private int DefaultPort;
        private string DefaultHttp;

        const string Authorization = "Authorization";
        const string AuthorizationToken = "Token";

        [SetUp]
        public void SetupWebServer()
        {
            Port++;
            DefaultPort = Port;
            DefaultHttp = "http://localhost:" + DefaultPort;
        }

        [Test]
        public async Task AuthenticationTest()
        {
            using (var webserver = new WebServer(DefaultPort))
            {
                var responseObj = new Dictionary<string, object> {{ AuthorizationToken, "123"}};

                webserver.RegisterModule(new FallbackModule((srv, ctx) =>
                {
                    if (ctx.RequestFormDataDictionary().ContainsKey("grant_type"))
                    {
                        ctx.JsonResponse(responseObj);
                    }

                    return true;
                }));

                var task = webserver.RunAsync();

                var data = await JsonClient.Authenticate(DefaultHttp, "admin", "password");

                Assert.IsNotNull(data);
                Assert.IsTrue(data.ContainsKey(AuthorizationToken));
                Assert.AreEqual(responseObj[AuthorizationToken], data[AuthorizationToken]);
            }
        }

        [Test]
        public async Task PostTest()
        {
            using (var webserver = new WebServer(DefaultPort))
            {
                const string status = "OK";

                webserver.RegisterModule(new FallbackModule((srv, ctx) =>
                {
                    var obj = ctx.ParseJson<BasicJson>();
                    Assert.IsNotNull(obj);
                    obj.StringData = status;
                    ctx.JsonResponse(obj);

                    return true;
                }));

                var task = webserver.RunAsync();

                var data = await JsonClient.Post<BasicJson>(DefaultHttp, BasicJson.GetDefault());

                Assert.IsNotNull(data);
                Assert.AreEqual(status, data.StringData);
            }
        }

        [Test]
        public async Task PostWithAuthenticationTest()
        {
            using (var webserver = new WebServer(DefaultPort))
            {
                webserver.RegisterModule(new FallbackModule((srv, ctx) =>
                {
                    ctx.JsonResponse(new Dictionary<string, string> {{Authorization, ctx.RequestHeader(Authorization) } });

                    return true;
                }));

                var task = webserver.RunAsync();

                var data = await JsonClient.Post(DefaultHttp, BasicJson.GetDefault(), AuthorizationToken);

                Assert.IsNotNull(data);
                Assert.IsTrue(data.ContainsKey(Authorization));
                Assert.AreEqual($"Bearer {AuthorizationToken}", data[Authorization]);
            }
        }

        [Test]
        public async Task GetWithAuthenticationTest()
        {
            using (var webserver = new WebServer(DefaultPort))
            {
                var ctxHeaders = new List<string>();

                webserver.RegisterModule(new FallbackModule((srv, ctx) =>
                {
                    foreach (var header in ctx.Request.Headers)
                        ctxHeaders.Add(header.ToString());

                    return true;
                }));

                var task = webserver.RunAsync();

                await JsonClient.GetAsString(DefaultHttp, AuthorizationToken);
                
                Assert.IsTrue(ctxHeaders.Any());
                Assert.IsTrue(ctxHeaders.Any(x => x.StartsWith(Authorization)));
            }
        }

        [Test]
        public async Task ThrowGetErrorTest()
        {
            await Task.Delay(0);
            Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await JsonClient.GetAsString(DefaultHttp);
            });
        }
    }
}