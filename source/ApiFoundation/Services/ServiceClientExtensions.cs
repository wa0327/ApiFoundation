using System;
using System.Net.Http;

namespace ApiFoundation.Services
{
    public static class ServiceClientExtensions
    {
        public static void Send<TRequest, TResponse>(this ApiClient source, HttpMethod method, string requestUri, Func<TRequest> requestCreator, Action<TResponse> responseParser)
            where TRequest : class
            where TResponse : class
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            if (requestUri == null)
            {
                throw new ArgumentNullException("requestUri");
            }

            // 有給 delegate 時才建立 request
            var request = default(TRequest);
            if (requestCreator != null)
            {
                request = requestCreator();
            }

            var response = source.Send<TRequest, TResponse>(method, requestUri, request);

            // 有給 delegate 時才處理 response
            if (responseParser != null)
            {
                responseParser(response);
            }
        }

        public static void Post<TRequest, TResponse>(this ApiClient source, string requestUri, Func<TRequest> requestCreator, Action<TResponse> responseParser)
            where TRequest : class
            where TResponse : class
        {
            source.Send(HttpMethod.Post, requestUri, requestCreator, responseParser);
        }

        public static void Get<TResponse>(this ApiClient source, string requestUri, Action<TResponse> responseParser)
            where TResponse : class
        {
            source.Send<object, TResponse>(HttpMethod.Get, requestUri, null, responseParser);
        }
    }
}