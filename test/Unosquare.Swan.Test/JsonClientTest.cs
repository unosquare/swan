using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Modules;
using Unosquare.Swan.Exceptions;
using Unosquare.Swan.Networking;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class JsonClientTest
    {
        private static int _port = 8080;
        private int _defaultPort;
        private string _defaultHttp;

        private const string Authorization = "Authorization";
        private const string AuthorizationToken = "Token";

        [SetUp]
        public void SetupWebServer()
        {
            _port++;
            _defaultPort = _port;
            _defaultHttp = "http://localhost:" + _defaultPort;
        }

        [Test]
        public async Task AuthenticationTest()
        {
            using (var webserver = new WebServer(_defaultPort))
            {
                var responseObj = new Dictionary<string, object> {{AuthorizationToken, "123"}};

                webserver.RegisterModule(new FallbackModule((ctx, ct) =>
                {
                    if (ctx.RequestFormDataDictionary().ContainsKey("grant_type"))
                    {
                        ctx.JsonResponse(responseObj);
                    }

                    return true;
                }));

                webserver.RunAsync();

                var data = await JsonClient.Authenticate(_defaultHttp, "admin", "password");

                Assert.IsNotNull(data);
                Assert.IsTrue(data.ContainsKey(AuthorizationToken));
                Assert.AreEqual(responseObj[AuthorizationToken], data[AuthorizationToken]);
            }
        }

        [Test]
        public async Task PostTest()
        {
            using (var webserver = new WebServer(_defaultPort))
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

                var data = await JsonClient.Post<BasicJson>(_defaultHttp, BasicJson.GetDefault());

                Assert.IsNotNull(data);
                Assert.AreEqual(status, data.StringData);
            }
        }

        [Test]
        public async Task PostWithAuthenticationTest()
        {
            using (var webserver = new WebServer(_defaultPort))
            {
                webserver.RegisterModule(new FallbackModule((ctx, ct) =>
                {
                    ctx.JsonResponse(new Dictionary<string, string> {{Authorization, ctx.RequestHeader(Authorization)}});

                    return true;
                }));

                webserver.RunAsync();
                await Task.Delay(500);

                var data = await JsonClient.Post(_defaultHttp, BasicJson.GetDefault(), AuthorizationToken);

                Assert.IsNotNull(data);
                Assert.IsTrue(data.ContainsKey(Authorization));
                Assert.AreEqual($"Bearer {AuthorizationToken}", data[Authorization]);
            }
        }

        [Test]
        public async Task GetWithAuthenticationTest()
        {
            using (var webserver = new WebServer(_defaultPort))
            {
                var ctxHeaders = new List<string>();

                webserver.RegisterModule(new FallbackModule((ctx, ct) =>
                {
                    ctxHeaders.AddRange(ctx.Request.Headers.Cast<object>().Select(header => header.ToString()));

                    return true;
                }));

                webserver.RunAsync();
                await Task.Delay(100);

                await JsonClient.GetString(_defaultHttp, AuthorizationToken);

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
                await JsonClient.GetString(_defaultHttp);
            });

            Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await JsonClient.Get<BasicJson>(_defaultHttp);
            });

            Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await JsonClient.GetBinary(_defaultHttp);
            });
        }

        [Test]
        public async Task PutTest()
        {
            using (var webserver = new WebServer(_defaultPort))
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

                var data = await JsonClient.Put<BasicJson>(_defaultHttp, BasicJson.GetDefault());

                Assert.IsNotNull(data);
                Assert.AreEqual(status, data.StringData);
            }
        }

        [Test]
        public async Task PutWithAuthenticationTest()
        {
            using (var webserver = new WebServer(_defaultPort))
            {
                webserver.RegisterModule(new FallbackModule((ctx, ct) =>
                {
                    ctx.JsonResponse(new Dictionary<string, string> {{Authorization, ctx.RequestHeader(Authorization)}});

                    return true;
                }));

                webserver.RunAsync();
                await Task.Delay(500);

                var data = await JsonClient.Put(_defaultHttp, BasicJson.GetDefault(), AuthorizationToken);

                Assert.IsNotNull(data);
                Assert.IsTrue(data.ContainsKey(Authorization));
                Assert.AreEqual($"Bearer {AuthorizationToken}", data[Authorization]);
            }
        }

        [Test]
        public async Task PostFileStringTest()
        {
            using (var webserver = new WebServer(_defaultPort))
            {
                var buffer = new byte[20];
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

                var data = await JsonClient.PostFileString(_defaultHttp, buffer, nameof(PostFileStringTest));

                Assert.IsNotNull(data);
            }
        }

        [Test]
        public async Task PostFileTest()
        {
            using (var webserver = new WebServer(_defaultPort))
            {
                var buffer = new byte[20];
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

                var data = await JsonClient.PostFile<JsonFile>(_defaultHttp, buffer, nameof(PostFileStringTest));

                Assert.IsNotNull(data);
                Assert.AreEqual(data.Filename, nameof(PostFileStringTest));
            }
        }


        [Test]
        public async Task PostOrErrorTest()
        {
            using (var webserver = new WebServer(_defaultPort))
            {
                webserver.RegisterModule(new FallbackModule((ctx, ct) =>
                {
                    var obj = ctx.ParseJson<BasicJson>();

                    if (obj.IntData == 1)
                    {
                        ctx.JsonResponse(obj);
                    }
                    else
                    {
                        ctx.Response.StatusCode = 500;
                        ctx.JsonResponse(new ErrorJson {Message = "ERROR"});
                    }

                    return true;
                }));

                webserver.RunAsync();
                await Task.Delay(100);

                var data = await JsonClient.PostOrError<BasicJson, ErrorJson>(_defaultHttp, new BasicJson {IntData = 1});

                Assert.IsNotNull(data);
                Assert.IsTrue(data.IsOk);
                Assert.IsNotNull(data.Ok);
                Assert.AreEqual(1, data.Ok.IntData);

                var dataError = await JsonClient.PostOrError<BasicJson, ErrorJson>(_defaultHttp,
                    new BasicJson {IntData = 2});

                Assert.IsNotNull(dataError);
                Assert.IsFalse(dataError.IsOk);
                Assert.IsNotNull(dataError.Error);
                Assert.AreEqual("ERROR", dataError.Error.Message);
            }
        }

        [Test]
        public void JsonRequestErrorTest()
        {
            var exception = Assert.ThrowsAsync<JsonRequestException>(async () =>
            {
                using (var webserver = new WebServer(_defaultPort))
                {
                    webserver.RegisterModule(new FallbackModule((ctx, ct) => false));

                    webserver.RunAsync();
                    await Task.Delay(100);

                    await JsonClient.Post<BasicJson>(_defaultHttp, BasicJson.GetDefault());
                }
            });
            
            Assert.AreEqual(404, exception.HttpErrorCode, "EmebedIO should return 404 error code");
        }
    }
}