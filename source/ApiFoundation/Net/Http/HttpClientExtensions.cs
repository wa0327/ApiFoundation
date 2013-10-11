using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http;

namespace ApiFoundation.Net.Http
{
    public static class HttpClientExtensions
    {
        private static readonly MediaTypeFormatter Json = new JsonMediaTypeFormatter();
        private static readonly MediaTypeFormatter Xml = new XmlMediaTypeFormatter();

        public static TResponseContent Send<TRequestContent, TResponseContent>(this HttpClient source, HttpMethod method, string requestUri, TRequestContent requestContent, MediaTypeFormatter formatter)
            where TRequestContent : class
            where TResponseContent : class
        {
            var request = new HttpRequestMessage(method, requestUri);

            if (requestContent != null)
            {
                var content = new ObjectContent<TRequestContent>(requestContent, formatter);
                request.Content = content;
            }

            var mediaType = formatter.MediaTypeMappings[0].MediaType.MediaType;
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));

            HttpResponseMessage response = null;
            try
            {
                response = source.SendAsync(request, CancellationToken.None).Result;
            }
            catch (AggregateException exception) // AggregateException 屬於多工的例外，因為這裡不使用多工處理，所以應進一步取出原始例外。
            {
                throw exception.InnerException;
            }

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return default(TResponseContent);
                }

                return response.Content.ReadAsAsync<TResponseContent>().Result;
            }
            else
            {
                HttpError httpError = null;
                try
                {
                    httpError = response.Content.ReadAsAsync<HttpError>().Result;
                }
                catch (Exception)
                {
                    var message = response.Content.ReadAsStringAsync().Result;
                    throw new HttpRequestException(message);
                }

                // model state errors
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new InvalidModelException(httpError);
                }

                // handled error
                if (response.StatusCode == HttpStatusCode.NotAcceptable)
                {
                    throw new InvocationNotAcceptableException(httpError);
                }

                // other errors
                throw new HttpServiceException(response.StatusCode, httpError);
            }
        }

        public static void Send<TRequestContent, TResponseContent>(this HttpClient source, HttpMethod method, string requestUri, MediaTypeFormatter formatter, Func<TRequestContent> requestContentCreator, Action<TResponseContent> responseContentProcessor)
            where TRequestContent : class
            where TResponseContent : class
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
            var requestContent = default(TRequestContent);
            if (requestContentCreator != null)
            {
                requestContent = requestContentCreator();
            }

            var responseContent = source.Send<TRequestContent, TResponseContent>(method, requestUri, requestContent, formatter);

            // 有給 delegate 時才處理 response
            if (responseContentProcessor != null)
            {
                responseContentProcessor(responseContent);
            }
        }

        public static void PostJson<TRequest, TResponse>(this HttpClient source, string requestUri, Func<TRequest> requestCreator, Action<TResponse> responseParser)
            where TRequest : class
            where TResponse : class
        {
            source.Send(HttpMethod.Post, requestUri, Json, requestCreator, responseParser);
        }

        public static void GetJson<TResponse>(this HttpClient source, string requestUri, Action<TResponse> responseParser)
            where TResponse : class
        {
            source.Send<object, TResponse>(HttpMethod.Get, requestUri, Json, null, responseParser);
        }
    }
}