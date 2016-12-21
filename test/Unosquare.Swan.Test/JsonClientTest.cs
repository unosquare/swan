using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Modules;
using Unosquare.Swan.Test.Mocks;
using Unosquare.Swan.Utilities;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class JsonClientTest
    {
        const string Authorization = "Authorization";
        const string AuthorizationToken = "Token";
        const string DefaultHttp = "http://localhost:8080";

        [Test]
        public async Task AuthenticationTest()
        {
            using (var webserver = new WebServer(8080))
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

                webserver.RunAsync();

                var data = await JsonClient.Authenticate(DefaultHttp, "admin", "password");

                Assert.IsNotNull(data);
                Assert.IsTrue(data.ContainsKey(AuthorizationToken));
                Assert.AreEqual(responseObj[AuthorizationToken], data[AuthorizationToken]);
            }
        }

        [Test]
        public async Task PostTest()
        {
            using (var webserver = new WebServer(8080))
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

                webserver.RunAsync();

                var data = await JsonClient.Post<BasicJson>(DefaultHttp, BasicJson.GetDefault());

                Assert.IsNotNull(data);
                Assert.AreEqual(status, data.StringData);
            }
        }

        [Test]
        public async Task PostWithAuthenticationTest()
        {
            using (var webserver = new WebServer(8080))
            {
                webserver.RegisterModule(new FallbackModule((srv, ctx) =>
                {
                    ctx.JsonResponse(new Dictionary<string, string> {{Authorization, ctx.RequestHeader(Authorization) } });

                    return true;
                }));

                webserver.RunAsync();

                var data = await JsonClient.Post(DefaultHttp, BasicJson.GetDefault(), AuthorizationToken);

                Assert.IsNotNull(data);
                Assert.IsTrue(data.ContainsKey(Authorization));
                Assert.AreEqual($"Bearer {AuthorizationToken}", data[Authorization]);
            }
        }

        [Test]
        public async Task GetWithAuthenticationTest()
        {
            using (var webserver = new WebServer(8080))
            {
                var ctxHeaders = new List<string>();

                webserver.RegisterModule(new FallbackModule((srv, ctx) =>
                {
                    foreach (var header in ctx.Request.Headers)
                        ctxHeaders.Add(header.ToString());

                    return true;
                }));

                webserver.RunAsync();

                var data = await JsonClient.GetAsString(DefaultHttp, AuthorizationToken);
                
                Assert.IsTrue(ctxHeaders.Any());
                Assert.IsTrue(ctxHeaders.Any(x => x.StartsWith(Authorization)));
            }
        }

        [Test]
        public async Task ThrowGetErrorTest()
        {
            Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await JsonClient.GetAsString(DefaultHttp);
            });
        }
    }
}
