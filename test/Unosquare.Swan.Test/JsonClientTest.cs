namespace Unosquare.Swan.Test.JsonClientTest
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using System.Threading.Tasks;
    using Exceptions;
    using Networking;
    using Mocks;

    public abstract class JsonClientTest
    {
        protected const string Authorization = "Authorization";
        protected const string AuthorizationToken = "Token";
        protected const string DefaultHttp = "http://localhost:3000";
    }

    [TestFixture]
    public class Authenticate : JsonClientTest
    {
        [Test]
        public async Task WithValidParams_ReturnsTrue()
        {
            var responseObj = new Dictionary<string, object> {{AuthorizationToken, "123"}};

            var data = await JsonClient.Authenticate(DefaultHttp + "/Authenticate", "admin", "password");

            Assert.IsNotNull(data);
            Assert.IsTrue(data.ContainsKey(AuthorizationToken));
            Assert.AreEqual(responseObj[AuthorizationToken], data[AuthorizationToken]);
        }

        [Test]
        public void WithInvalidParams_ThrowsSecurityException()
        {
            Assert.ThrowsAsync<SecurityException>(async () =>
                await JsonClient.Authenticate(DefaultHttp + "/511", "admin", "password"));
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
                await JsonClient.Authenticate(DefaultHttp, null, "password"));
        }
    }

    [TestFixture]
    public class Post : JsonClientTest
    {
        private string _api = "/Post";

        [Test]
        public async Task WithValidParams_ReturnsTrue()
        {
            const string status = "OK";
            var basicJson = BasicJson.GetDefault();

            var data = await JsonClient.Post<BasicJson>(DefaultHttp + _api + "/WithValidParams", basicJson);

            Assert.IsNotNull(data);
            Assert.AreEqual(status, data.StringData);
        }

        [Test]
        public async Task WithValidParamsAndAuthorizationToken_ReturnsTrue()
        {
            var data = await JsonClient.Post(DefaultHttp + _api + "/WithValidParamsAndAuthorizationToken",
                BasicJson.GetDefault(), AuthorizationToken);

            Assert.IsNotNull(data);
            Assert.IsTrue(data.ContainsKey(Authorization));
            Assert.AreEqual($"Bearer {AuthorizationToken}", data[Authorization]);
        }

        [Test]
        public void WithInvalidParams_ThrowsJsonRequestException()
        {
            var exception = Assert.ThrowsAsync<JsonRequestException>(async () =>
            {
                await JsonClient.Post<BasicJson>(DefaultHttp + "/404", BasicJson.GetDefault());
            });

            Assert.AreEqual(404, exception.HttpErrorCode);
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
        private string _api = "/GetString";

        [Test]
        public async Task WithValidParamsAndAuthorizationToken_ReturnsTrue()
        {
            var jsonString = await JsonClient.GetString(DefaultHttp + _api + "/WithValidParamsAndAuthorizationToken",
                AuthorizationToken);

            Assert.IsNotEmpty(jsonString);
            Assert.IsTrue(jsonString.Contains(Authorization.ToLower()));
        }

        [Test]
        public void WithInvalidParam_ThrowsJsonRequestException()
        {
            Assert.ThrowsAsync<JsonRequestException>(async () =>
                await JsonClient.GetString(DefaultHttp + _api + "/InvalidParam"));
        }
    }

    [TestFixture]
    public class Put : JsonClientTest
    {
        private string _api = "/Put";

        [Test]
        public async Task WithValidParams_ReturnsTrue()
        {
            const string status = "OK";

            var data = await JsonClient.Put<BasicJson>(DefaultHttp + _api + "/WithValidParams", BasicJson.GetDefault());

            Assert.IsNotNull(data);
            Assert.AreEqual(status, data.StringData);
        }

        [Test]
        public async Task WithValidParamsAndAuthorizationToken_ReturnsTrue()
        {
            var data = await JsonClient.Put(DefaultHttp + _api + "/WithValidParamsAndAuthorizationToken",
                BasicJson.GetDefault(), AuthorizationToken);

            Assert.IsNotNull(data);
            Assert.IsTrue(data.ContainsKey(Authorization));
            Assert.AreEqual($"Bearer {AuthorizationToken}", data[Authorization]);
        }

        [Test]
        public void WithInvalidParams_ThrowsJsonRequestException()
        {
            var exception = Assert.ThrowsAsync<JsonRequestException>(async () =>
            {
                await JsonClient.Put<BasicJson>(DefaultHttp + "/404", BasicJson.GetDefault());
            });

            Assert.AreEqual(404, exception.HttpErrorCode);
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
        private string _api = "/PostFileString";

        [Test]
        public async Task WithValidParams_ReturnsTrue()
        {
            var buffer = new byte[20];
            new Random().NextBytes(buffer);

            var data = await JsonClient.PostFileString(DefaultHttp + _api + "/WithValidParams", buffer,
                nameof(WithValidParams_ReturnsTrue));

            Assert.IsNotEmpty(data);
        }
    }

    [TestFixture]
    public class PostFile : JsonClientTest
    {
        private string _api = "/PostFile";

        [Test]
        public async Task WithValidParams_ReturnsTrue()
        {
            var buffer = new byte[20];
            new Random().NextBytes(buffer);

            var data = await JsonClient.PostFile<JsonFile>(DefaultHttp + _api + "/WithValidParams", buffer,
                nameof(WithValidParams_ReturnsTrue));

            Assert.IsNotNull(data);
            Assert.AreEqual(data.Filename, nameof(WithValidParams_ReturnsTrue));
        }
    }

    [TestFixture]
    public class PostOrError : JsonClientTest
    {
        private string _api = "/PostOrError";

        [TestCase(1, 500, true)]
        [TestCase(2, 500, false)]
        [TestCase(4678, 404, false)]
        public async Task PostOrErrorTest(int input, int error, bool expected)
        {
            var data = await JsonClient.PostOrError<BasicJson, ErrorJson>(
                DefaultHttp + _api + "/PostOrErrorTest",
                new BasicJson {IntData = input},
                error);

            Assert.IsNotNull(data);
            Assert.AreEqual(expected, data.IsOk);
        }
    }

    [TestFixture]
    public class GetBinary : JsonClientTest
    {
        private string _api = "/GetBinary";

        [Test]
        public void WithNullUrl_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await JsonClient.GetBinary(null));
        }

        [Test]
        public async Task WithValidParams_ReturnsTrue()
        {
            var headers = await JsonClient.GetBinary(DefaultHttp + _api + "/WithValidParams");

            Assert.IsTrue(headers.Any());
        }

        [Test]
        public void WithInvalidUrl_ThrowsJsonRequestException()
        {
            Assert.ThrowsAsync<JsonRequestException>(async () =>
                await JsonClient.GetBinary(DefaultHttp + "/InvalidParam"));
        }
    }

    [TestFixture]
    public class Get : JsonClientTest
    {
        private string _api = "/Get";

        [Test]
        public void WithNullUrl_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await JsonClient.Get<BasicJson>(null));
        }

        [Test]
        public async Task WithValidParams_ReturnsTrue()
        {
            var basicJson = await JsonClient.Get<BasicJson>(DefaultHttp + _api + "/WithValidParams");

            Assert.IsNotNull(basicJson);
        }
    }
}