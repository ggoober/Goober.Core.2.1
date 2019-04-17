using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Goober.Core.Utils
{
    public static class ApiUtils
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new StringEnumConverter() },
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        public static string BuildUrl(string schemeAndHost, string urlPath)
        {
            var baseUri = new UriBuilder(schemeAndHost);

            return new UriBuilder(scheme: baseUri.Scheme,
                                                host: baseUri.Host,
                                                port: baseUri.Port,
                                                path: urlPath,
                                                extraValue: string.Empty).Uri.ToString();
        }

        public static async Task<T> ExecuteGetAsync<T>(string url, int? timeout = null)
        {
            using (var client = new HttpClient())
            {
                if (timeout.HasValue)
                {
                    client.Timeout = TimeSpan.FromMilliseconds(timeout.Value);
                }

                var str = await client.GetStringAsync(new Uri(url));

                return JsonConvert.DeserializeObject<T>(str, JsonSerializerSettings);
            }
        }

        public static async Task<T> ExecutePostAsync<T, U>(string url, U data, int? timeout = null)
        {
            var parameters = JsonConvert.SerializeObject(data, JsonSerializerSettings);

            return await ExecutePostAsync<T>(url, parameters, timeout);
        }

        public static async Task<T> ExecutePostAsync<T>(string url, string parameters, int? timeout = null)
        {
            using (var client = new HttpClient())
            {
                if (timeout.HasValue)
                {
                    client.Timeout = TimeSpan.FromMilliseconds(timeout.Value);
                }

                var content = new StringContent(parameters, Encoding.UTF8, "application/json");

                var httpResponse = client.PostAsync(url, content).Result;

                return await ProcessResponseMessageAsync<T>(httpResponse, url, parameters);
            }
        }

        public static async Task<TResponse> ExecutePostAsync<TResponse, TRequest>(string url, TRequest request, KeyValuePair<string, string> authParameter, int? timeout = null)
        {
            var httpRequest = new HttpRequestMessage();
            var requestJson = JsonConvert.SerializeObject(request);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue(authParameter.Key, authParameter.Value);
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpRequest.Method = HttpMethod.Post;
            httpRequest.RequestUri = new Uri(url);
            httpRequest.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var httpResponse = await client.SendAsync(httpRequest);
                return await ProcessResponseMessageAsync<TResponse>(httpResponse, url, requestJson);
            }
        }

        public static async Task<TResponse> ExecutePostAsync<TResponse>(string url, IEnumerable<KeyValuePair<string, string>> formContentRequest, KeyValuePair<string, string> authParameter, int? timeout = null)
        {
            var httpRequest = new HttpRequestMessage();
            var requestJson = JsonConvert.SerializeObject(formContentRequest);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue(authParameter.Key, authParameter.Value);
            httpRequest.Method = HttpMethod.Post;
            httpRequest.RequestUri = new Uri(url);
            httpRequest.Content = new FormUrlEncodedContent(formContentRequest);

            using (var client = new HttpClient())
            {
                var httpResponse = await client.SendAsync(httpRequest);
                return await ProcessResponseMessageAsync<TResponse>(httpResponse, url, requestJson);
            }
        }

        public static async Task<byte[]> ExecutePostWithByteArrayResponseAsync<TRequest>(string url, TRequest parameters, int? timeout = null)
        {
            using (var client = new HttpClient())
            {
                if (timeout.HasValue)
                {
                    client.Timeout = TimeSpan.FromMilliseconds(timeout.Value);
                }

                var jsonSerialized = JsonConvert.SerializeObject(parameters);

                var content = new StringContent(jsonSerialized, Encoding.UTF8, "application/json");

                var httpResponse = client.PostAsync(url, content).Result;

                return await ProcessByteArrayResponseMessageAsync(httpResponse, url, jsonSerialized);
            }
        }

        #region private methods

        private static async Task<T> ProcessResponseMessageAsync<T>(HttpResponseMessage responseMessage, string url, string parameters)
        {
            if (responseMessage.StatusCode == HttpStatusCode.NoContent)
            {
                return default(T);
            }

            if (responseMessage.StatusCode != HttpStatusCode.OK)
                throw new WebException($"Request({url}) fault with code = {responseMessage.StatusCode}, data: {parameters}");

            var jsonString = await responseMessage.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        private static async Task<byte[]> ProcessByteArrayResponseMessageAsync(HttpResponseMessage httpResponseMessage, string url, string parameters)
        {
            if (httpResponseMessage.StatusCode == HttpStatusCode.NoContent)
            {
                return new byte[0];
            }

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                throw new WebException($"Request({url}) fault with code = {httpResponseMessage.StatusCode}, parameters = {parameters}");

            return await httpResponseMessage.Content.ReadAsByteArrayAsync();
        }
        
        #endregion
    }
}
