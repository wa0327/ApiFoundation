using System;
using System.Net.Http;

namespace ApiFoundation.Services
{
    public static class HttpClientExtensions
    {
        public static void Send<TRequest, TResponse>(this IApiClient source, HttpMethod method, string requestUri, Func<TRequest> requestCreator, Action<TResponse> responseParser)
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

        public static void Post<TRequest, TResponse>(this IApiClient source, string requestUri, Func<TRequest> requestCreator, Action<TResponse> responseParser)
            where TRequest : class
            where TResponse : class
        {
            source.Send(HttpMethod.Post, requestUri, requestCreator, responseParser);
        }

        public static void Get<TRequest, TResponse>(this IApiClient source, string requestUri, Func<TRequest> requestCreator, Action<TResponse> responseParser)
            where TRequest : class
            where TResponse : class
        {
            source.Send(HttpMethod.Get, requestUri, requestCreator, responseParser);
        }
    }
}