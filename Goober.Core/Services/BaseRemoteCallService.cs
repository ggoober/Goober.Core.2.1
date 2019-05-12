using Goober.Core.Utils;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Goober.Core.Services
{
    public abstract class BaseRemoteCallService
    {
        #region fields

        protected virtual int _defaultCacheTime { get; set; } = 5;

        protected abstract string _apiSchemeAndHostConfigKey { get; set; }

        protected readonly ICacheProvider _cacheProvider;

        private readonly IConfiguration _configuration;

        #endregion

        #region ctor

        public BaseRemoteCallService(ICacheProvider cacheProvider,
            IConfiguration configuration)
        {
            _cacheProvider = cacheProvider;
            _configuration = configuration;
        }

        #endregion
        
        protected async Task<TResponse> ExecutePostAsync<TResponse, TRequest>(string path, TRequest request, int? timeoutMiliseconds = null)
        {
            var schemeAndHost = GetSchemeAndHostFromConfigs();

            var url = ApiUtils.BuildUrl(schemeAndHost: schemeAndHost, urlPath: path);

            var result = await ApiUtils.ExecutePostAsync<TResponse, TRequest>(url: url, 
                                            data: request, 
                                            timeout: timeoutMiliseconds);

            return result;
        }

        protected async Task<byte[]> ExecutePostReturnsByteArrayAsync<TRequest>(string path, TRequest request, int? timeoutMiliseconds = null)
        {
            var schemeAndHost = GetSchemeAndHostFromConfigs();

            var url = ApiUtils.BuildUrl(schemeAndHost: schemeAndHost, urlPath: path);

            var result = await ApiUtils.ExecutePostWithByteArrayResponseAsync<TRequest>(url: url,
                                            parameters: request,
                                            timeout: timeoutMiliseconds);

            return result;
        }

        protected async Task<TResponse> ExecuteGetAsync<TResponse>(string pathAndQuery, int? timeoutMiliseconds = null)
        {
            var schemeAndHost = GetSchemeAndHostFromConfigs();

            var url = ApiUtils.BuildUrl(schemeAndHost: schemeAndHost, urlPath: pathAndQuery);

            var result = await ApiUtils.ExecuteGetAsync<TResponse>(url: url, timeout: timeoutMiliseconds);

            return result;
        }

        private string GetSchemeAndHostFromConfigs()
        {
            if (string.IsNullOrEmpty(_apiSchemeAndHostConfigKey) == true)
                throw new InvalidOperationException("ApiSchemeAndHostConfigKey is empty");

            var schemeAndHost = _configuration[_apiSchemeAndHostConfigKey];

            if (string.IsNullOrEmpty(schemeAndHost))
                throw new InvalidOperationException($"schemeAndHost is empty by key = {_apiSchemeAndHostConfigKey}");
            return schemeAndHost;
        }
    }
}
