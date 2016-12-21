using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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
        /// <param name="url"></param>
        /// <param name="payload"></param>
        /// <param name="authorization"></param>
        /// <returns></returns>
        public static async Task<T> Post<T>(string url, object payload, string authorization = null)
        {
            using (var httpClient = new HttpClient())
            {
                if (string.IsNullOrWhiteSpace(authorization) == false)
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authorization);
                
                var payloadJson = new StringContent(JsonFormatter.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, payloadJson);

                if (response.IsSuccessStatusCode == false)
                    throw new Exception($"Error POST Json. Status code: {response.StatusCode}");

                return JsonFormatter.Deserialize<T>(await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// Gets the specified URL and return the JSON data as object
        /// with optional authorization token.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="authorization">The authorization.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static async Task<T> Get<T>(string url, string authorization = null)
        {
            using (var httpClient = new HttpClient())
            {
                if (string.IsNullOrWhiteSpace(authorization) == false)
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authorization);
                
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode == false)
                    throw new Exception($"Error POST Json. Status code: {response.StatusCode}");

                return JsonFormatter.Deserialize<T>(await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// Gets the binary.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="authorization">The authorization.</param>
        /// <returns></returns>
        public static async Task<byte[]> GetBinary(string url, string authorization = null)
        {
            using (var httpClient = new HttpClient())
            {
                if (string.IsNullOrWhiteSpace(authorization) == false)
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authorization);

                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode == false)
                    throw new Exception($"Error POST Json. Status code: {response.StatusCode}");

                return await response.Content.ReadAsByteArrayAsync();
            }
        }
        
        /// <summary>
        /// Authenticate
        /// </summary>
        /// <param name="url"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<IDictionary<string, object>> Authenticate(string url, string username, string password)
        {
            using (var httpClient = new HttpClient())
            {
                var requestContent = new StringContent($"grant_type=password&username={username}&password={password}",
                    Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = await httpClient.PostAsync(url, requestContent);

                if (response.IsSuccessStatusCode == false)
                    throw new Exception($"Error Authenticating. Status code: {response.StatusCode}");

                return JsonFormatter.Deserialize(await response.Content.ReadAsStringAsync());
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
            return await Post<string>(url, new { Filename = fileName, Data = image }, authorization);
        }
    }
}
