using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.Swan.Formatters;

namespace Unosquare.Swan.Utilities
{
    /// <summary>
    /// Represents a HttpClient with extended methods to use with JSON payloads 
    /// and bearer tokens authentication
    /// </summary>
    public class JsonClient
    {
        /// <summary>
        /// Post a object as JSON with optional authorization token.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static async Task<T> Post<T>(string url, object payload, string authorization = null,
            CancellationToken ct = default(CancellationToken))
        {
            var jsonString = await PostAsString(url, payload, authorization, ct);

            return string.IsNullOrEmpty(jsonString) ? default(T) : JsonFormatter.Deserialize<T>(jsonString);
        }

        /// <summary>
        /// Posts the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<IDictionary<string, object>> Post(string url, object payload,
            string authorization = null, CancellationToken ct = default(CancellationToken))
        {
            var jsonString = await PostAsString(url, payload, authorization, ct);

            return string.IsNullOrEmpty(jsonString)
                ? default(IDictionary<string, object>)
                : JsonFormatter.Deserialize(jsonString);
        }

        /// <summary>
        /// Posts the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static async Task<string> PostAsString(string url, object payload, string authorization = null,
            CancellationToken ct = default(CancellationToken))
        {
            using (var httpClient = new HttpClient())
            {
                if (string.IsNullOrWhiteSpace(authorization) == false)
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authorization);

                var payloadJson = new StringContent(JsonFormatter.Serialize(payload), Encoding.UTF8, "application/json");

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
        public static async Task<T> Put<T>(string url, object payload, string authorization = null,
            CancellationToken ct = default(CancellationToken))
        {
            var jsonString = await PutAsString(url, payload, authorization, ct);

            return string.IsNullOrEmpty(jsonString) ? default(T) : JsonFormatter.Deserialize<T>(jsonString);
        }

        /// <summary>
        /// Puts the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<IDictionary<string, object>> Put(string url, object payload,
            string authorization = null, CancellationToken ct = default(CancellationToken))
        {
            var jsonString = await PutAsString(url, payload, authorization, ct);

            return string.IsNullOrEmpty(jsonString)
                ? default(IDictionary<string, object>)
                : JsonFormatter.Deserialize(jsonString);
        }

        /// <summary>
        /// Puts as string.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static async Task<string> PutAsString(string url, object payload, string authorization = null,
            CancellationToken ct = default(CancellationToken))
        {
            using (var httpClient = new HttpClient())
            {
                if (string.IsNullOrWhiteSpace(authorization) == false)
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authorization);

                var payloadJson = new StringContent(JsonFormatter.Serialize(payload), Encoding.UTF8, "application/json");

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
        /// <exception cref="System.Exception"></exception>
        public static async Task<string> GetAsString(string url, string authorization = null,
            CancellationToken ct = default(CancellationToken))
        {
            using (var httpClient = new HttpClient())
            {
                if (string.IsNullOrWhiteSpace(authorization) == false)
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authorization);

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
        /// <exception cref="System.Exception"></exception>
        public static async Task<T> Get<T>(string url, string authorization = null,
            CancellationToken ct = default(CancellationToken))
        {
            var jsonString = await GetAsString(url, authorization, ct);
            return string.IsNullOrEmpty(jsonString) ? default(T) : JsonFormatter.Deserialize<T>(jsonString);
        }

        /// <summary>
        /// Gets the binary.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="authorization">The authorization.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static async Task<byte[]> GetBinary(string url, string authorization = null,
            CancellationToken ct = default(CancellationToken))
        {
            using (var httpClient = new HttpClient())
            {
                if (string.IsNullOrWhiteSpace(authorization) == false)
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authorization);

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
        /// <exception cref="Exception"></exception>
        public static async Task<IDictionary<string, object>> Authenticate(string url, string username, string password,
            CancellationToken ct = default(CancellationToken))
        {
            using (var httpClient = new HttpClient())
            {
                var requestContent = new StringContent($"grant_type=password&username={username}&password={password}",
                    Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = await httpClient.PostAsync(url, requestContent, ct);

                if (response.IsSuccessStatusCode == false)
                    throw new Exception($"Error Authenticating. Status code: {response.StatusCode}");

                var jsonPayload = await response.Content.ReadAsStringAsync();

                return JsonFormatter.Deserialize(jsonPayload);
            }
        }

        /// <summary>
        /// Posts the file.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="image">The image.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="authorization">The authorization.</param>
        /// <returns></returns>
        public static async Task<string> PostFile(string url, byte[] image, string fileName, string authorization = null)
        {
            return await Post<string>(url, new {Filename = fileName, Data = image}, authorization);
        }
    }
}