namespace Swan.Net
{
    using Formatters;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a HttpClient with extended methods to use with JSON payloads 
    /// and bearer tokens authentication.
    /// </summary>
    public static class JsonClient
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private const string JsonMimeType = "application/json";
        private const string FormType = "application/x-www-form-urlencoded";

        /// <summary>
        /// Post a object as JSON with optional authorization token.
        /// </summary>
        /// <typeparam name="T">The type of response object.</typeparam>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task with a result of the requested type.
        /// </returns>
        public static async Task<T> Post<T>(
            string requestUri,
            object payload,
            string? authorization = null,
            CancellationToken cancellationToken = default)
        {
            var jsonString = await PostString(requestUri, payload, authorization, cancellationToken)
                .ConfigureAwait(false);

            return !string.IsNullOrEmpty(jsonString) ? Json.Deserialize<T>(jsonString) : default;
        }

        /// <summary>
        /// Posts the specified URL.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task with a result as a collection of key/value pairs.
        /// </returns>
        public static async Task<IDictionary<string, object>> Post(
            string requestUri,
            object payload,
            string? authorization = null,
            CancellationToken cancellationToken = default)
        {
            var jsonString = await PostString(requestUri, payload, authorization, cancellationToken)
                .ConfigureAwait(false);

            return string.IsNullOrWhiteSpace(jsonString)
                ? default
                : Json.Deserialize(jsonString) as IDictionary<string, object>;
        }

        /// <summary>
        /// Posts the specified URL.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task with a result of the requested string.
        /// </returns>
        /// <exception cref="ArgumentNullException">url.</exception>
        /// <exception cref="JsonRequestException">Error POST JSON.</exception>
        public static Task<string> PostString(
            string requestUri,
            object payload,
            string? authorization = null,
            CancellationToken cancellationToken = default)
            => SendAsync(HttpMethod.Post, requestUri, payload, authorization, cancellationToken);

        /// <summary>
        /// Puts the specified URL.
        /// </summary>
        /// <typeparam name="T">The type of response object.</typeparam>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A task with a result of the requested type.
        /// </returns>
        public static async Task<T> Put<T>(
            string requestUri,
            object payload,
            string? authorization = null,
            CancellationToken ct = default)
        {
            var jsonString = await PutString(requestUri, payload, authorization, ct)
                .ConfigureAwait(false);

            return !string.IsNullOrEmpty(jsonString) ? Json.Deserialize<T>(jsonString) : default;
        }

        /// <summary>
        /// Puts the specified URL.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task with a result of the requested collection of key/value pairs.
        /// </returns>
        public static async Task<IDictionary<string, object>?> Put(
            string requestUri,
            object payload,
            string? authorization = null,
            CancellationToken cancellationToken = default)
        {
            var response = await Put<object>(requestUri, payload, authorization, cancellationToken)
                .ConfigureAwait(false);

            return response as IDictionary<string, object>;
        }

        /// <summary>
        /// Puts as string.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A task with a result of the requested string.
        /// </returns>
        /// <exception cref="ArgumentNullException">url.</exception>
        /// <exception cref="JsonRequestException">Error PUT JSON.</exception>
        public static Task<string> PutString(
            string requestUri,
            object payload,
            string? authorization = null,
            CancellationToken ct = default) => SendAsync(HttpMethod.Put, requestUri, payload, authorization, ct);

        /// <summary>
        /// Gets as string.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A task with a result of the requested string.
        /// </returns>
        /// <exception cref="ArgumentNullException">url.</exception>
        /// <exception cref="JsonRequestException">Error GET JSON.</exception>
        public static Task<string> GetString(
            string requestUri,
            string? authorization = null,
            CancellationToken ct = default)
            => GetString(new Uri(requestUri), null, authorization, ct);

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The ct.</param>
        /// <returns>
        /// A task with a result of the requested string.
        /// </returns>
        public static async Task<string> GetString(
            Uri uri,
            IDictionary<string, IEnumerable<string>> headers,
            string? authorization = null,
            CancellationToken ct = default)
        {
            var response = await GetHttpContent(uri, ct, authorization, headers)
                .ConfigureAwait(false);

            return await response.ReadAsStringAsync()
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the specified URL and return the JSON data as object
        /// with optional authorization token.
        /// </summary>
        /// <typeparam name="T">The response type.</typeparam>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A task with a result of the requested type.
        /// </returns>
        public static async Task<T> Get<T>(
            string requestUri,
            string? authorization = null,
            CancellationToken ct = default)
        {
            var jsonString = await GetString(requestUri, authorization, ct)
                .ConfigureAwait(false);

            return !string.IsNullOrEmpty(jsonString) ? Json.Deserialize<T>(jsonString) : default;
        }

        /// <summary>
        /// Gets the binary.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A task with a result of the requested byte array.
        /// </returns>
        /// <exception cref="ArgumentNullException">url.</exception>
        /// <exception cref="JsonRequestException">Error GET Binary.</exception>
        public static async Task<byte[]> GetBinary(
            string requestUri,
            string? authorization = null,
            CancellationToken ct = default)
        {
            var response = await GetHttpContent(new Uri(requestUri), ct, authorization)
                .ConfigureAwait(false);

            return await response.ReadAsByteArrayAsync()
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Authenticate against a web server using Bearer Token.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A task with a Dictionary with authentication data.
        /// </returns>
        /// <exception cref="ArgumentNullException">url
        /// or
        /// username.</exception>
        /// <exception cref="SecurityException">Error Authenticating.</exception>
        public static async Task<IDictionary<string, object>> Authenticate(
            string requestUri,
            string username,
            string password,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(requestUri))
                throw new ArgumentNullException(nameof(requestUri));

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            // ignore empty password for now
            var content = $"grant_type=password&username={username}&password={password}";
            using var requestContent = new StringContent(content, Encoding.UTF8, FormType);
            var response = await HttpClient.PostAsync(requestUri, requestContent, ct).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new SecurityException($"Error Authenticating. Status code: {response.StatusCode}.");

            var jsonPayload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return Json.Deserialize(jsonPayload) as IDictionary<string, object>;
        }

        /// <summary>
        /// Posts the file.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A task with a result of the requested string.
        /// </returns>
        public static Task<string> PostFileString(
            string requestUri,
            byte[] buffer,
            string fileName,
            string? authorization = null,
            CancellationToken ct = default) =>
            PostString(requestUri, new { Filename = fileName, Data = buffer }, authorization, ct);

        /// <summary>
        /// Posts the file.
        /// </summary>
        /// <typeparam name="T">The response type.</typeparam>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A task with a result of the requested string.
        /// </returns>
        public static Task<T> PostFile<T>(
            string requestUri,
            byte[] buffer,
            string fileName,
            string? authorization = null,
            CancellationToken ct = default) =>
            Post<T>(requestUri, new { Filename = fileName, Data = buffer }, authorization, ct);

        /// <summary>
        /// Sends the asynchronous request.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A task with a result of the requested string.
        /// </returns>
        /// <exception cref="ArgumentNullException">requestUri.</exception>
        /// <exception cref="JsonRequestException">Error {method} JSON.</exception>
        public static async Task<string> SendAsync(
            HttpMethod method,
            string requestUri,
            object payload,
            string? authorization = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(requestUri))
                throw new ArgumentNullException(nameof(requestUri));

            using var response = await GetResponse(new Uri(requestUri), authorization, null, payload, method, ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new JsonRequestException(
                    $"Error {method} JSON",
                    (int)response.StatusCode,
                    await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }

            return await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
        }

        private static async Task<HttpContent> GetHttpContent(
            Uri uri,
            CancellationToken ct,
            string? authorization = null,
            IDictionary<string, IEnumerable<string>> headers = null)
        {
            var response = await GetResponse(uri, authorization, headers, ct)
                .ConfigureAwait(false);

            return response.IsSuccessStatusCode
                ? response.Content
                : throw new JsonRequestException("Error GET", (int)response.StatusCode);
        }

        private static async Task<HttpResponseMessage> GetResponse(
            Uri uri,
            string authorization,
            IDictionary<string, IEnumerable<string>> headers,
            object payload = null,
            HttpMethod method = default,
            CancellationToken ct = default)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var httpMethod = method ?? HttpMethod.Get;

            using var requestMessage = new HttpRequestMessage(httpMethod, uri);
            if (!string.IsNullOrWhiteSpace(authorization))
            {
                requestMessage.Headers.Authorization
                    = new AuthenticationHeaderValue("Bearer", authorization);
            }

            if (headers != null)
            {
                foreach (var header in headers)
                    requestMessage.Headers.Add(header.Key, header.Value);
            }

            if (payload != null && httpMethod != HttpMethod.Get)
            {
                requestMessage.Content = new StringContent(Json.Serialize(payload), Encoding.UTF8, JsonMimeType);
            }

            return await HttpClient.SendAsync(requestMessage, ct)
                .ConfigureAwait(false);
        }
    }
}
