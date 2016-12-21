using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
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
        const string authorization = "Authorization";
        const string authorizationToken = "Token";

        [Test]
        public async Task AuthenticationTest()
        {
            using (var webserver = new WebServer(8080))
            {
                var responseObj = new Dictionary<string, object> {{ authorizationToken, "123"}};

                webserver.RegisterModule(new FallbackModule((srv, ctx) =>
                {
                    if (ctx.RequestFormDataDictionary().ContainsKey("grant_type"))
                    {
                        ctx.JsonResponse(responseObj);
                    }

                    return true;
                }));

                webserver.RunAsync();

                var data = await JsonClient.Authenticate($"http://localhost:8080", "admin", "password");

                Assert.IsNotNull(data);
                Assert.IsTrue(data.ContainsKey(authorizationToken));
                Assert.AreEqual(responseObj[authorizationToken], data[authorizationToken]);
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

                var data = await JsonClient.Post<BasicJson>("http://localhost:8080", BasicJson.GetDefault());

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
                    ctx.JsonResponse(new Dictionary<string, string> {{authorization, ctx.RequestHeader(authorization) } });

                    return true;
                }));

                webserver.RunAsync();

                var data = await JsonClient.Post("http://localhost:8080", BasicJson.GetDefault(), authorizationToken);

                Assert.IsNotNull(data);
                Assert.IsTrue(data.ContainsKey(authorization));
                Assert.AreEqual($"Bearer {authorizationToken}", data[authorization]);
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

                var data = await JsonClient.GetAsString("http://localhost:8080", authorizationToken);
                
                Assert.IsTrue(ctxHeaders.Any());
                Assert.IsTrue(ctxHeaders.Any(x => x.StartsWith(authorization)));
            }
        }
    }
}
