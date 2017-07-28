namespace Unosquare.Swan.Networking
{
    using Models;
    using Formatters;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a HttpClient with extended methods to use with JSON payloads 
    /// and bearer tokens authentication
    /// </summary>
    public class JsonClient
    {
        private const string JsonMimeType = "application/json";

        #region Methods

        /// <summary>
        /// Post a object as JSON with optional authorization token.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Represents errors that occur during application execution</exception>
        public static async Task<T> Post<T>(
            string url, 
            object payload, 
            string authorization = null,
            CancellationToken ct = default(CancellationToken))
        {
            var jsonString = await PostString(url, payload, authorization, ct);

            return string.IsNullOrEmpty(jsonString) ? default(T) : Json.Deserialize<T>(jsonString);
        }

        /// <summary>
        /// Posts a object as JSON with optional authorization token and retrieve an object
        /// or an error.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TE">The type of the e.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="httpStatusError">The HTTP status error.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<OkOrError<T, TE>> PostOrError<T, TE>(
            string url, 
            object payload,
            int httpStatusError = 500, 
            string authorization = null,
            CancellationToken ct = default(CancellationToken))
        {
            using (var httpClient = GetHttpClientWithAuthorizationHeader(authorization))
            {
                var payloadJson = new StringContent(Json.Serialize(payload), Encoding.UTF8, JsonMimeType);

                var response = await httpClient.PostAsync(url, payloadJson, ct);

                var jsonString = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return new OkOrError<T, TE>
                    {
                        IsOk = true,
                        Ok = string.IsNullOrEmpty(jsonString) ? default(T) : Json.Deserialize<T>(jsonString)
                    };
                }

                if ((int)response.StatusCode == httpStatusError)
                {
                    return new OkOrError<T, TE>
                    {
                        Error = string.IsNullOrEmpty(jsonString) ? default(TE) : Json.Deserialize<TE>(jsonString)
                    };
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
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<IDictionary<string, object>> Post(
            string url, 
            object payload,
            string authorization = null, 
            CancellationToken ct = default(CancellationToken))
        {
            var jsonString = await PostString(url, payload, authorization, ct);

            return string.IsNullOrWhiteSpace(jsonString)
                ? default(IDictionary<string, object>)
                : Json.Deserialize(jsonString) as IDictionary<string, object>;
        }

        /// <summary>
        /// Posts the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Represents errors that occur during application execution</exception>
        public static async Task<string> PostString(
            string url, 
            object payload, 
            string authorization = null,
            CancellationToken ct = default(CancellationToken))
        {
            using (var httpClient = GetHttpClientWithAuthorizationHeader(authorization))
            {
                var payloadJson = new StringContent(Json.Serialize(payload), Encoding.UTF8, JsonMimeType);

                var response = await httpClient.PostAsync(url, payloadJson, ct);

                if (response.IsSuccessStatusCode == false)
                    throw new Exception($"Error POST Json. Status code: {response.StatusCode}");

                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// Puts the specified URL.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<T> Put<T>(
            string url, 
            object payload, 
            string authorization = null,
            CancellationToken ct = default(CancellationToken))
        {
            var jsonString = await PutString(url, payload, authorization, ct);

            return string.IsNullOrEmpty(jsonString) ? default(T) : Json.Deserialize<T>(jsonString);
        }

        /// <summary>
        /// Puts the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<IDictionary<string, object>> Put(
            string url, 
            object payload,
            string authorization = null, 
            CancellationToken ct = default(CancellationToken))
        {
            var jsonString = await PutString(url, payload, authorization, ct);

            return string.IsNullOrEmpty(jsonString)
                ? default(IDictionary<string, object>)
                : Json.Deserialize(jsonString) as IDictionary<string, object>;
        }

        /// <summary>
        /// Puts as string.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Represents errors that occur during application execution</exception>
        public static async Task<string> PutString(
            string url, 
            object payload, 
            string authorization = null,
            CancellationToken ct = default(CancellationToken))
        {
            using (var httpClient = GetHttpClientWithAuthorizationHeader(authorization))
            {
                var payloadJson = new StringContent(Json.Serialize(payload), Encoding.UTF8, JsonMimeType);

                var response = await httpClient.PutAsync(url, payloadJson, ct);

                if (response.IsSuccessStatusCode == false)
                    throw new Exception($"Error POST Json. Status code: {response.StatusCode}");

                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// Gets as string.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Represents errors that occur during application execution</exception>
        public static async Task<string> GetString(
            string url, 
            string authorization = null,
            CancellationToken ct = default(CancellationToken))
        {
            using (var httpClient = GetHttpClientWithAuthorizationHeader(authorization))
            {
                var response = await httpClient.GetAsync(url, ct);

                if (response.IsSuccessStatusCode == false)
                    throw new Exception($"Error POST Json. Status code: {response.StatusCode}");

                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// Gets the specified URL and return the JSON data as object
        /// with optional authorization token.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Represents errors that occur during application execution</exception>
        public static async Task<T> Get<T>(
            string url, 
            string authorization = null,
            CancellationToken ct = default(CancellationToken))
        {
            var jsonString = await GetString(url, authorization, ct);
            return string.IsNullOrEmpty(jsonString) ? default(T) : Json.Deserialize<T>(jsonString);
        }

        /// <summary>
        /// Gets the binary.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Represents errors that occur during application execution</exception>
        public static async Task<byte[]> GetBinary(
            string url, 
            string authorization = null,
            CancellationToken ct = default(CancellationToken))
        {
            using (var httpClient = GetHttpClientWithAuthorizationHeader(authorization))
            {
                var response = await httpClient.GetAsync(url, ct);

                if (response.IsSuccessStatusCode == false)
                    throw new Exception($"Error POST Json. Status code: {response.StatusCode}");

                return await response.Content.ReadAsByteArrayAsync();
            }
        }

        /// <summary>
        /// Authenticate
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Represents errors that occur during application execution</exception>
        public static async Task<IDictionary<string, object>> Authenticate(
            string url, 
            string username, 
            string password,
            CancellationToken ct = default(CancellationToken))
        {
            using (var httpClient = new HttpClient())
            {
                var requestContent = new StringContent(
                    $"grant_type=password&username={username}&password={password}",
                    Encoding.UTF8, 
                    "application/x-www-form-urlencoded");
                var response = await httpClient.PostAsync(url, requestContent, ct);

                if (response.IsSuccessStatusCode == false)
                    throw new SecurityException($"Error Authenticating. Status code: {response.StatusCode}");

                var jsonPayload = await response.Content.ReadAsStringAsync();

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
        /// <returns></returns>
        public static async Task<string> PostFileString(
            string url, 
            byte[] buffer, 
            string fileName,
            string authorization = null)
        {
            return await PostString(url, new { Filename = fileName, Data = buffer }, authorization);
        }

        /// <summary>
        /// Posts the file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="authorization">The authorization.</param>
        /// <returns></returns>
        public static async Task<T> PostFile<T>(string url, byte[] buffer, string fileName, string authorization = null)
        {
            return await Post<T>(url, new { Filename = fileName, Data = buffer }, authorization);
        }

        #endregion

        #region Private Methods

        private static HttpClient GetHttpClientWithAuthorizationHeader(string authorization)
        {
            var httpClient = new HttpClient();

            if (string.IsNullOrWhiteSpace(authorization) == false)
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authorization);
            }

            return httpClient;
        }

        #endregion
    }
}