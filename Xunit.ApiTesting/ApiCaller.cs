using System;
using System.Net;
using RestSharp;

namespace Xunit.ApiTesting
{
    public class ApiCaller
    {
        private readonly string _baseUrl;
        private readonly Action<RestRequest> _addValidCredentials;
        private readonly Action<RestRequest> _addInvalidCredentials;

        public ApiCaller(string baseUrl, Action<RestRequest> addValidCredentials, Action<RestRequest> addInvalidCredentials )
        {
            _baseUrl = baseUrl;
            _addValidCredentials = addValidCredentials;
            _addInvalidCredentials = addInvalidCredentials;
        }
        public IRestResponse<T> Get<T>(
            string url,
            Action<RestRequest> config = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            int timeout = 300,
            bool useValidApiKey = true) where T : new()
        {
            return Call<T>(url, Method.GET, config, expectedStatusCode, timeout, useValidApiKey);
        }

        public IRestResponse Get(
            string url,
            Action<RestRequest> config = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            int timeout = 300,
            bool useValidApiKey = true)
        {
            return Call(url, Method.GET, config, expectedStatusCode, timeout, useValidApiKey);
        }

        public IRestResponse<T> Post<T>(
            string url,
            Action<RestRequest> config = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            int timeout = 300,
            bool useValidApiKey = true) where T : new()
        {
            return Call<T>(url, Method.POST, config, expectedStatusCode, timeout, useValidApiKey);
        }

        public IRestResponse Post(
            string url,
            Action<RestRequest> config = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            int timeout = 300,
            bool useValidApiKey = true)
        {
            return Call(url, Method.POST, config, expectedStatusCode, timeout, useValidApiKey);
        }

        public IRestResponse<T> Put<T>(
            string url,
            Action<RestRequest> config = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            int timeout = 300,
            bool useValidApiKey = true) where T : new()
        {
            return Call<T>(url, Method.PUT, config, expectedStatusCode, timeout, useValidApiKey);
        }

        public IRestResponse Put(
            string url,
            Method method = Method.POST,
            Action<RestRequest> config = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            int timeout = 300,
            bool useValidApiKey = true)
        {
            return Call(url, Method.PUT, config, expectedStatusCode, timeout, useValidApiKey);
        }

        
        public IRestResponse<T> Delete<T>(
            string url,
            Action<RestRequest> config = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            int timeout = 300,
            bool useValidApiKey = true) where T : new()
        {
            return Call<T>(url, Method.DELETE, config, expectedStatusCode, timeout, useValidApiKey);
        }

        public IRestResponse Delete(
            string url,
            Method method = Method.POST,
            Action<RestRequest> config = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            int timeout = 300,
            bool useValidApiKey = true)
        {
            return Call(url, Method.DELETE, config, expectedStatusCode, timeout, useValidApiKey);
        }

        private IRestResponse<T> Call<T>(
            string url,
            Method method,
            Action<RestRequest> config = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            int timeout = 300,
            bool useValidCredentials = true)
            where T : new()
        {
            var client = new RestClient(_baseUrl);
            var request = new RestRequest(url, method);
            
            config?.Invoke(request);
            request.Timeout = timeout;
            AddCredentials(request, useValidCredentials);
            var response = client.Execute<T>(request);
            EnsureNoTimeout(request, response);
            EnsureStatusCode(response, expectedStatusCode);
            return response;
        }

        private IRestResponse Call(
            string url,
            Method method,
            Action<RestRequest> config = null,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK,
            int timeout = 300,
            bool useValidCredentials = true)
        {
            var client = new RestClient(_baseUrl);
            var request = new RestRequest(url, method);

            config?.Invoke(request);
            request.Timeout = timeout;
            AddCredentials(request, useValidCredentials);
            var response = client.Execute(request);
            EnsureNoTimeout(request, response);
            EnsureStatusCode(response, expectedStatusCode);
            return response;
        }

        private static void EnsureNoTimeout(RestRequest request, IRestResponse response)
        {
            if (response.ResponseStatus == ResponseStatus.TimedOut || response.StatusCode == 0)
            {
                throw new TimeoutException($"Timeoutexception for {request.Resource}, {request.Method}, "
                                           + $"the request took longer than the allowed {request.Timeout} ms");
            }
        }

        
        public static void EnsureStatusCode<T>(IRestResponse<T> response, HttpStatusCode statusCode)
        {
            Assert.Equal(statusCode, response.StatusCode);
        }

        public static void EnsureStatusCode(IRestResponse response, HttpStatusCode statusCode)
        {
            Assert.Equal(statusCode, response.StatusCode);
        }
        
        private void AddCredentials(RestRequest request, bool useValidCredentials)
        {
            if (useValidCredentials)
            {
                _addValidCredentials(request);
            }
            else
            {
                _addInvalidCredentials(request);
            }
        }
    }
}
