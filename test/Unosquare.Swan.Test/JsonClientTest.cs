namespace Unosquare.Swan.Test.JsonClientTest
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using System.Threading.Tasks;
    using Unosquare.Labs.EmbedIO;
    using Unosquare.Labs.EmbedIO.Modules;
    using Unosquare.Swan.Exceptions;
    using Unosquare.Swan.Networking;
    using Unosquare.Swan.Test.Mocks;

    public abstract class JsonClientTest
    {
        protected static int _port = 8080;
        protected int _defaultPort;
        protected string _defaultHttp;

        protected const string Authorization = "Authorization";
        protected const string AuthorizationToken = "Token";

        [SetUp]
        public void SetupWebServer()
        {
            _port++;
            _defaultPort = _port;
            _defaultHttp = "http://localhost:" + _defaultPort;
        }

    }

    [TestFixture]
    public class Authenticate : JsonClientTest
    {
        [Test]
        public async Task WithValidParams_ReturnsTrue()
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
                await Task.Delay(100);

                var data = await JsonClient.Authenticate(_defaultHttp, "admin", "password");

                Assert.IsNotNull(data);
                Assert.IsTrue(data.ContainsKey(AuthorizationToken));
                Assert.AreEqual(responseObj[AuthorizationToken], data[AuthorizationToken]);
            }
        }

        [Test]
        public void WithInvalidParams_ThrowsSecurityException()
        {
            Assert.ThrowsAsync<SecurityException>(async () =>
            {
                using (var webserver = new WebServer(_defaultPort))
                {
                    webserver.RegisterModule(new FallbackModule((ctx, ct) => false));
                    webserver.RunAsync();
                    await Task.Delay(100);

                    var data = await JsonClient.Authenticate(_defaultHttp, "admin", "password");
                }
            });
        }

        [Test]
        public void WithNullUrl_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await JsonClient.Authenticate(null, "admin", "password"));
        }

        [Test]
        public void WithNullUsername_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await JsonClient.Authenticate(_defaultHttp, null, "password"));
        }
    }

    [TestFixture]
    public class Post : JsonClientTest
    {
        [Test]
        public async Task WithValidParams_ReturnsTrue()
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
        public async Task WithValidParamsAndAuthorizationToken_ReturnsTrue()
        {
            using (var webserver = new WebServer(_defaultPort))
            {
                webserver.RegisterModule(new FallbackModule((ctx, ct) =>
                {
                    ctx.JsonResponse(new Dictionary<string, string>
                    {
                        {Authorization, ctx.RequestHeader(Authorization)}
                    });

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
        public void WithInvalidParams_ThrowsJsonRequestException()
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

        [Test]
        public void WithNullUrl_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await JsonClient.Post<BasicJson>(null, BasicJson.GetDefault()));
        }
    }

    [TestFixture]
    public class GetString : JsonClientTest
    {
        [Test]
        public async Task WithValidParamsAndAuthorizationToken_ReturnsTrue()
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
        public async Task WithInvalidParam_ThrowsHttpRequestException()
        {
            Assert.ThrowsAsync<System.Net.Http.HttpRequestException>(async () =>
                await JsonClient.GetString(_defaultHttp));
        }
    }

    [TestFixture]
    public class Put : JsonClientTest
    {
        [Test]
        public async Task WithValidParams_ReturnsTrue()
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
        public async Task WithValidParamsAndAuthorizationToken_ReturnsTrue()
        {
            using (var webserver = new WebServer(_defaultPort))
            {
                webserver.RegisterModule(new FallbackModule((ctx, ct) =>
                {
                    ctx.JsonResponse(new Dictionary<string, string>
                    {
                        {Authorization, ctx.RequestHeader(Authorization)}
                    });

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
        public void WithInvalidParams_ThrowsJsonRequestException()
        {
            var exception = Assert.ThrowsAsync<JsonRequestException>(async () =>
            {
                using (var webserver = new WebServer(_defaultPort))
                {
                    webserver.RegisterModule(new FallbackModule((ctx, ct) => false));
                    webserver.RunAsync();
                    await Task.Delay(100);

                    await JsonClient.Put<BasicJson>(_defaultHttp, BasicJson.GetDefault());
                }
            });

            Assert.AreEqual(404, exception.HttpErrorCode, "EmebedIO should return 404 error code");
        }

        [Test]
        public void WithNullUrl_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await JsonClient.Put<BasicJson>(null, BasicJson.GetDefault()));
        }
    }

    [TestFixture]
    public class PostFileString : JsonClientTest
    {
        [Test]
        public async Task WithValidParams_ReturnsTrue()
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

                var data = await JsonClient.PostFileString(_defaultHttp, buffer, nameof(WithValidParams_ReturnsTrue));

                Assert.IsNotNull(data);
            }
        }

    }

    [TestFixture]
    public class PostFile : JsonClientTest
    {
        [Test]
        public async Task WithValidParams_ReturnsTrue()
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

                var data = await JsonClient.PostFile<JsonFile>(_defaultHttp, buffer, "Paco De Lucia");

                Assert.IsNotNull(data);
                Assert.AreEqual(data.Filename, "Paco De Lucia");
            }
        }
    }

    [TestFixture]
    public class PostOrError : JsonClientTest
    {

        [TestCase(1, 500, true)]
        [TestCase(2, 500, false)]
        [TestCase(4678, 404, false)]
        public async Task PostOrErrorTest(int input, int error, bool expected)
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

                var data = await JsonClient.PostOrError<BasicJson, ErrorJson>(_defaultHttp,
                    new BasicJson {IntData = input}, error);

                Assert.IsNotNull(data);
                Assert.AreEqual(expected, data.IsOk);

            }
        }
    }

    [TestFixture]
    public class GetBinary : JsonClientTest
    {
        [Test]
        public async Task WithInvalidParams_ThrowsHttpRequestException()
        {
            Assert.ThrowsAsync<System.Net.Http.HttpRequestException>(async () =>
                await JsonClient.GetBinary(_defaultHttp));
        }

        [Test]
        public async Task WithNullUrl_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await JsonClient.GetBinary(null));
        }

        [Test]
        public async Task WithValidParams_ReturnsTrue()
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

                await JsonClient.GetBinary(_defaultHttp);

                Assert.IsTrue(ctxHeaders.Any());
            }
        }

        [Test]
        public async Task WithInvalidUrl_ThrowsJsonRequestException()
        {
            Assert.ThrowsAsync<JsonRequestException>(async () =>
            {
                var data = await JsonClient.GetBinary("https://accesscore.azurewebsites.net/api/token");
            });
        }
    }

    [TestFixture]
    public class Get : JsonClientTest
    {
        [Test]
        public async Task WithInvalidParams_ThrowsHttpRequestException()
        {
            Assert.ThrowsAsync<System.Net.Http.HttpRequestException>(async () =>
                await JsonClient.Get<BasicJson>(_defaultHttp));
        }

        [Test]
        public async Task WithNullUrl_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await JsonClient.Get<BasicJson>(null));
        }

        [Test]
        public async Task WithValidParams_ReturnsTrue()
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

                var arc = await JsonClient.Get<BasicJson>(_defaultHttp);

                Assert.IsTrue(ctxHeaders.Any());
            }
        }
    }
}