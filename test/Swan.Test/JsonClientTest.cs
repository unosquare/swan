namespace Swan.Test.JsonClientTest
{
    using Mocks;
    using Swan.Net;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using System.Threading.Tasks;

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

            var data = await JsonClient.Authenticate(new Uri($"{DefaultHttp}/Authenticate"), "admin", "password");

            Assert.IsNotNull(data);
            Assert.IsTrue(data!.ContainsKey(AuthorizationToken));
            Assert.AreEqual(responseObj[AuthorizationToken], data[AuthorizationToken]);
            //Assert.That(data[AuthorizationToken], Is.EqualTo(responseObj[AuthorizationToken]));
        }

        [Test]
        public void WithInvalidParams_ThrowsSecurityException()
        {
            Assert.ThrowsAsync<SecurityException>(async () =>
                await JsonClient.Authenticate(new Uri($"{DefaultHttp}/511"), "admin", "password"));
        }

        [Test]
        public void WithNullUsername_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await JsonClient.Authenticate(new Uri(DefaultHttp), null, "password"));
        }
    }

    [TestFixture]
    public class Post : JsonClientTest
    {
        private const string Api = "/Post";

        [Test]
        public async Task WithValidParams_ReturnsTrue()
        {
            const string status = "OK";
            var basicJson = BasicJson.GetDefault();

            var data = await JsonClient.Post<BasicJson>(new Uri($"{DefaultHttp}{Api}/WithValidParams"), basicJson);

            Assert.IsNotNull(data);
            Assert.AreEqual(status, data.StringData);
        }

        [Test]
        public async Task WithValidParamsAndAuthorizationToken_ReturnsTrue()
        {
            var data = await JsonClient.Post(
                new Uri($"{DefaultHttp}{Api}/WithValidParamsAndAuthorizationToken"),
                BasicJson.GetDefault(),
                AuthorizationToken);

            Assert.IsNotNull(data);
            Assert.IsTrue(data!.ContainsKey(Authorization));
            Assert.AreEqual($"Bearer {AuthorizationToken}", data[Authorization]);
        }

        [Test]
        public void WithInvalidParams_ThrowsJsonRequestException()
        {
            var exception = Assert.ThrowsAsync<JsonRequestException>(async () =>
            {
                await JsonClient.Post<BasicJson>(new Uri($"{DefaultHttp}/404"), BasicJson.GetDefault());
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
        private const string Api = "/GetString";

        [Test]
        public async Task WithValidParamsAndAuthorizationToken_ReturnsTrue()
        {
            var jsonString = await JsonClient.GetString(new Uri(
                    $"{DefaultHttp}{Api}/WithValidParamsAndAuthorizationToken"),
                AuthorizationToken);

            Assert.IsNotEmpty(jsonString);
            Assert.IsTrue(jsonString.Contains(Authorization.ToLower()));
        }

        [Test]
        public void WithInvalidParam_ThrowsJsonRequestException()
        {
            Assert.ThrowsAsync<JsonRequestException>(async () =>
                await JsonClient.GetString(new Uri(DefaultHttp + Api + "/InvalidParam")));
        }
    }

    [TestFixture]
    public class Put : JsonClientTest
    {
        private const string Api = "/Put";

        [Test]
        public async Task WithValidParams_ReturnsTrue()
        {
            const string status = "OK";

            var data = await JsonClient.Put<BasicJson>(new Uri($"{DefaultHttp}{Api}/WithValidParams"), BasicJson.GetDefault());

            Assert.IsNotNull(data);
            Assert.AreEqual(status, data.StringData);
        }

        [Test]
        public async Task WithValidParamsAndAuthorizationToken_ReturnsTrue()
        {
            var data = await JsonClient.Put(
                new Uri($"{DefaultHttp}{Api}/WithValidParamsAndAuthorizationToken"),
                BasicJson.GetDefault(),
                AuthorizationToken);

            Assert.IsNotNull(data);
            Assert.IsTrue(data!.ContainsKey(Authorization));
            Assert.AreEqual($"Bearer {AuthorizationToken}", data[Authorization]);
        }

        [Test]
        public void WithInvalidParams_ThrowsJsonRequestException()
        {
            var exception = Assert.ThrowsAsync<JsonRequestException>(async () =>
            {
                await JsonClient.Put<BasicJson>(new Uri($"{DefaultHttp}/404"), BasicJson.GetDefault());
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
        private const string Api = "/PostFileString";

        [Test]
        public async Task WithValidParams_ReturnsTrue()
        {
            var buffer = new byte[20];
            new Random().NextBytes(buffer);

            var data = await JsonClient.PostFileString(
                new Uri($"{DefaultHttp}{Api}/WithValidParams"),
                buffer,
                nameof(WithValidParams_ReturnsTrue));

            Assert.IsNotEmpty(data);
        }
    }

    [TestFixture]
    public class PostFile : JsonClientTest
    {
        private const string Api = "/PostFile";

        [Test]
        public async Task WithValidParams_ReturnsTrue()
        {
            var buffer = new byte[20];
            new Random().NextBytes(buffer);

            var data = await JsonClient.PostFile<JsonFile>(
                new Uri($"{DefaultHttp}{Api}/WithValidParams"),
                buffer,
                nameof(WithValidParams_ReturnsTrue));

            Assert.IsNotNull(data);
            Assert.AreEqual(data.Filename, nameof(WithValidParams_ReturnsTrue));
        }
    }

    [TestFixture]
    public class GetBinary : JsonClientTest
    {
        private const string Api = "/GetBinary";

        [Test]
        public void WithNullUrl_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await JsonClient.GetBinary(null));
        }

        [Test]
        public async Task WithValidParams_ReturnsTrue()
        {
            var headers = await JsonClient.GetBinary(new Uri($"{DefaultHttp}{Api}/WithValidParams"));

            Assert.IsTrue(headers.Any());
        }

        [Test]
        public void WithInvalidUrl_ThrowsJsonRequestException()
        {
            Assert.ThrowsAsync<JsonRequestException>(async () =>
                await JsonClient.GetBinary(new Uri($"{DefaultHttp}/InvalidParam")));
        }
    }

    [TestFixture]
    public class Get : JsonClientTest
    {
        private const string Api = "/Get";

        [Test]
        public void WithNullUrl_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await JsonClient.Get<BasicJson>(null));
        }

        [Test]
        public async Task WithValidParams_ReturnsTrue()
        {
            var basicJson = await JsonClient.Get<BasicJson>(new Uri($"{DefaultHttp}{Api}/WithValidParams"));

            Assert.IsNotNull(basicJson);
        }
    }
}
