using NUnit.Framework;
using System;
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

        private const string Authorization = "Authorization";
        private const string AuthorizationToken = "Token";

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

                webserver.RegisterModule(new FallbackModule((ctx, ct) =>
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
            using (var webserver = new WebServer(DefaultPort))
            {
                const string status = "OK";

                webserver.RegisterModule(new FallbackModule((ctx, ct) =>
                {
                    var obj = ctx.ParseJson<BasicJson>();
                    Assert.IsNotNull(obj);
                    obj.StringData = status;
                    ctx.JsonResponse(obj);

                    return true;
                }));

                webserver.RunAsync();
                await Task.Delay(100);

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
                webserver.RegisterModule(new FallbackModule((ctx, ct) =>
                {
                    ctx.JsonResponse(new Dictionary<string, string> {{Authorization, ctx.RequestHeader(Authorization) } });

                    return true;
                }));

                webserver.RunAsync();
                await Task.Delay(500);

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

                webserver.RegisterModule(new FallbackModule((ctx, ct) =>
                {
                    ctxHeaders.AddRange(ctx.Request.Headers.Cast<object>().Select(header => header.ToString()));

                    return true;
                }));

                webserver.RunAsync();
                await Task.Delay(100);

                await JsonClient.GetString(DefaultHttp, AuthorizationToken);
                
                Assert.IsTrue(ctxHeaders.Any());
                Assert.IsTrue(ctxHeaders.Any(x => x.StartsWith(Authorization)));
            }
        }

        [Test]
        public async Task ThrowGetErrorTest()
        {
            await Task.Delay(10);

            Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await JsonClient.GetString(DefaultHttp);
            });

            Assert.ThrowsAsync<HttpRequestException>(async () => 
            { 
                await JsonClient.Get<BasicJson>(DefaultHttp);
            });

            Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await JsonClient.GetBinary(DefaultHttp);
            });
        }

        [Test]
        public async Task PutTest()
        {
            using (var webserver = new WebServer(DefaultPort))
            {
                const string status = "OK";

                webserver.RegisterModule(new FallbackModule((ctx, ct) =>
                {
                    var obj = ctx.ParseJson<BasicJson>();
                    Assert.IsNotNull(obj);
                    obj.StringData = status;
                    ctx.JsonResponse(obj);

                    return true;
                }));

                webserver.RunAsync();
                await Task.Delay(100);

                var data = await JsonClient.Put<BasicJson>(DefaultHttp, BasicJson.GetDefault());

                Assert.IsNotNull(data);
                Assert.AreEqual(status, data.StringData);
            }
        }

        [Test]
        public async Task PutWithAuthenticationTest()
        {
            using (var webserver = new WebServer(DefaultPort))
            {
                webserver.RegisterModule(new FallbackModule((ctx, ct) =>
                {
                    ctx.JsonResponse(new Dictionary<string, string> { { Authorization, ctx.RequestHeader(Authorization) } });

                    return true;
                }));

                webserver.RunAsync();
                await Task.Delay(500);

                var data = await JsonClient.Put(DefaultHttp, BasicJson.GetDefault(), AuthorizationToken);

                Assert.IsNotNull(data);
                Assert.IsTrue(data.ContainsKey(Authorization));
                Assert.AreEqual($"Bearer {AuthorizationToken}", data[Authorization]);
            }
        }

        [Test]
        public async Task PostFileTest()
        {
            using (var webserver = new WebServer(DefaultPort))
            {
                byte[] buffer = new byte[20];
                new Random().NextBytes(buffer);

                webserver.RegisterModule(new FallbackModule((ctx, ct) =>
                {
                    var obj = ctx.ParseJson<JsonFile>();
                    Assert.IsNotNull(obj);
                    ctx.JsonResponse(obj);
                    return true;
                }));

                webserver.RunAsync();
                await Task.Delay(100);

                var data = await JsonClient.PostFile(DefaultHttp, buffer, "filename");
  
                Assert.IsNotNull(data);
            }
        }
    }
}