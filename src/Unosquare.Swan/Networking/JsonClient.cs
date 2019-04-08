namespace Unosquare.Swan.Networking
{
    using Exceptions;
    using Formatters;
    using Models;
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
        private const string JsonMimeType = "application/json";

        /// <summary>
        /// Post a object as JSON with optional authorization token.
        /// </summary>
        /// <typeparam name="T">The type of response object.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task with a result of the requested type.</returns>
        public static async Task<T> Post<T>(
            string url,
            object payload,
            string authorization = null,
            CancellationToken cancellationToken = default)
        {
            var jsonString = await PostString(url, payload, authorization, cancellationToken)
                .ConfigureAwait(false);

            return !string.IsNullOrEmpty(jsonString) ? Json.Deserialize<T>(jsonString) : default;
        }

        /// <summary>
        /// Posts a object as JSON with optional authorization token and retrieve an object
        /// or an error.
        /// </summary>
        /// <typeparam name="T">The type of response object.</typeparam>
        /// <typeparam name="TE">The type of the error.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="httpStatusError">The HTTP status error.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task with a result of the requested type or an error object.</returns>
        public static async Task<OkOrError<T, TE>> PostOrError<T, TE>(
            string url,
            object payload,
            int httpStatusError = 500,
            string authorization = null,
            CancellationToken cancellationToken = default)
        {
            using (var response = await GetResponse(new Uri(url), cancellationToken, authorization, null, payload, HttpMethod.Post).ConfigureAwait(false))
            {
                var jsonString = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return OkOrError<T, TE>.FromOk(!string.IsNullOrEmpty(jsonString)
                        ? Json.Deserialize<T>(jsonString)
                        : default);
                }

                if ((int)response.StatusCode == httpStatusError)
                {
                    return OkOrError<T, TE>.FromError(!string.IsNullOrEmpty(jsonString)
                        ? Json.Deserialize<TE>(jsonString)
                        : default);
                }

                return new OkOrError<T, TE>();
            }
        }

        /// <summary>
        /// Posts the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task with a result as a collection of key/value pairs.</returns>
        public static async Task<IDictionary<string, object>> Post(
            string url,
            object payload,
            string authorization = null,
            CancellationToken cancellationToken = default)
        {
            var jsonString = await PostString(url, payload, authorization, cancellationToken)
                .ConfigureAwait(false);

            return string.IsNullOrWhiteSpace(jsonString)
                ? default
                : Json.Deserialize(jsonString) as IDictionary<string, object>;
        }

        /// <summary>
        /// Posts the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task with a result of the requested string.
        /// </returns>
        /// <exception cref="ArgumentNullException">url.</exception>
        /// <exception cref="JsonRequestException">Error POST JSON.</exception>
        public static Task<string> PostString(
            string url,
            object payload,
            string authorization = null,
            CancellationToken cancellationToken = default)
            => SendAsync(HttpMethod.Post, url, payload, authorization, cancellationToken);

        /// <summary>
        /// Puts the specified URL.
        /// </summary>
        /// <typeparam name="T">The type of response object.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task with a result of the requested type.</returns>
        public static async Task<T> Put<T>(
            string url,
            object payload,
            string authorization = null,
            CancellationToken ct = default)
        {
            var jsonString = await PutString(url, payload, authorization, ct)
                .ConfigureAwait(false);

            return !string.IsNullOrEmpty(jsonString) ? Json.Deserialize<T>(jsonString) : default;
        }

        /// <summary>
        /// Puts the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task with a result of the requested collection of key/value pairs.
        /// </returns>
        public static async Task<IDictionary<string, object>> Put(
            string url,
            object payload,
            string authorization = null,
            CancellationToken cancellationToken = default)
        {
            var response = await Put<object>(url, payload, authorization, cancellationToken)
                .ConfigureAwait(false);

            return response as IDictionary<string, object>;
        }

        /// <summary>
        /// Puts as string.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A task with a result of the requested string.
        /// </returns>
        /// <exception cref="ArgumentNullException">url.</exception>
        /// <exception cref="JsonRequestException">Error PUT JSON.</exception>
        public static Task<string> PutString(
            string url,
            object payload,
            string authorization = null,
            CancellationToken ct = default) => SendAsync(HttpMethod.Put, url, payload, authorization, ct);

        /// <summary>
        /// Gets as string.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A task with a result of the requested string.
        /// </returns>
        /// <exception cref="ArgumentNullException">url.</exception>
        /// <exception cref="JsonRequestException">Error GET JSON.</exception>
        public static Task<string> GetString(
            string url,
            string authorization = null,
            CancellationToken ct = default)
            => GetString(new Uri(url), null, authorization, ct);

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
            string authorization = null,
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
        /// <param name="url">The URL.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task with a result of the requested type.</returns>
        public static async Task<T> Get<T>(
            string url,
            string authorization = null,
            CancellationToken ct = default)
        {
            var jsonString = await GetString(url, authorization, ct)
                .ConfigureAwait(false);

            return !string.IsNullOrEmpty(jsonString) ? Json.Deserialize<T>(jsonString) : default;
        }

        /// <summary>
        /// Gets the binary.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A task with a result of the requested byte array.
        /// </returns>
        /// <exception cref="ArgumentNullException">url.</exception>
        /// <exception cref="JsonRequestException">Error GET Binary.</exception>
        public static async Task<byte[]> GetBinary(
            string url,
            string authorization = null,
            CancellationToken ct = default)
        {
            var response = await GetHttpContent(new Uri(url), ct, authorization)
                .ConfigureAwait(false);

            return await response.ReadAsByteArrayAsync()
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Authenticate against a web server using Bearer Token.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A task with a Dictionary with authentication data.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// url
        /// or
        /// username.
        /// </exception>
        /// <exception cref="SecurityException">Error Authenticating.</exception>
        public static async Task<IDictionary<string, object>> Authenticate(
            string url,
            string username,
            string password,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            using (var httpClient = new HttpClient())
            {
                // ignore empty password for now
                var requestContent = new StringContent(
                    $"grant_type=password&username={username}&password={password}",
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded");
                var response = await httpClient.PostAsync(url, requestContent, ct).ConfigureAwait(false);

                if (response.IsSuccessStatusCode == false)
                    throw new SecurityException($"Error Authenticating. Status code: {response.StatusCode}.");

                var jsonPayload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                return Json.Deserialize(jsonPayload) as IDictionary<string, object>;
            }
        }

        /// <summary>
        /// Posts the file.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A task with a result of the requested string.
        /// </returns>
        public static Task<string> PostFileString(
            string url,
            byte[] buffer,
            string fileName,
            string authorization = null,
            CancellationToken ct = default) =>
            PostString(url, new { Filename = fileName, Data = buffer }, authorization, ct);

        /// <summary>
        /// Posts the file.
        /// </summary>
        /// <typeparam name="T">The response type.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task with a result of the requested string.</returns>
        public static Task<T> PostFile<T>(
            string url,
            byte[] buffer,
            string fileName,
            string authorization = null,
            CancellationToken ct = default) =>
            Post<T>(url, new { Filename = fileName, Data = buffer }, authorization, ct);

        /// <summary>
        /// Sends the asynchronous request.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task with a result of the requested string.</returns>
        public static async Task<string> SendAsync(
            HttpMethod method,
            string url,
            object payload,
            string authorization = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            using (var response = await GetResponse(new Uri(url), ct, authorization, null, payload, method).ConfigureAwait(false))
            {
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
        }

        private static async Task<HttpContent> GetHttpContent(
            Uri uri,
            CancellationToken ct,
            string authorization = null,
            IDictionary<string, IEnumerable<string>> headers = null)
        {
            var response = await GetResponse(uri, ct, authorization, headers)
                .ConfigureAwait(false);

            return response.IsSuccessStatusCode
                ? response.Content
                : throw new JsonRequestException("Error GET", (int)response.StatusCode);
        }

        private static async Task<HttpResponseMessage> GetResponse(
            Uri uri,
            CancellationToken ct,
            string authorization,
            IDictionary<string, IEnumerable<string>> headers,
            object payload = null,
            HttpMethod method = default)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var httpMethod = method ?? HttpMethod.Get;
            using (var httpClient = new HttpClient())
            {
                using (var requestMessage = new HttpRequestMessage(httpMethod, uri))
                {
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

                    return await httpClient.SendAsync(requestMessage, ct)
                        .ConfigureAwait(false);
                }
            }
        }
    }
}