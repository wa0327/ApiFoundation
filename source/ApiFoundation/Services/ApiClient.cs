using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using ApiFoundation.Net.Http;

namespace ApiFoundation.Services
{
    public class ApiClient : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly MediaTypeFormatter mediaFormatter;

        public ApiClient(HttpClient httpClient, MediaTypeFormatter formatter)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException("httpClient");
            }

            if (formatter == null)
            {
                throw new ArgumentNullException("formatter");
            }

            httpClient.InsertHandler(this.CreateMessageInterceptor());

            this.httpClient = httpClient;
            this.mediaFormatter = formatter;
        }

        public ApiClient(HttpClient httpClient)
            : this(httpClient, new JsonMediaTypeFormatter())
        {
        }

        public event EventHandler<HttpRequestEventArgs> SendingRequest;

        public event EventHandler<HttpResponseEventArgs> ResponseReceived;

        public void Dispose()
        {
            this.httpClient.Dispose();
        }

        /// <summary>
        /// 送出 HTTP 要求
        /// </summary>
        /// <typeparam name="TRequest">The type of the request.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="method">The method.</param>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="request">The request.</param>
        /// <returns>
        /// HTTP 回應
        /// </returns>
        /// <exception cref="ApiFoundation.Services.BadInvocationException">當呼叫端送出的格式有問題時擲出。</exception>
        /// <exception cref="ApiFoundation.Services.InvocationNotAcceptableException">當被呼叫端發生商業邏輯錯誤時擲出。</exception>
        /// <exception cref="ApiFoundation.Services.HttpServiceException">當被呼叫端發生非商業邏輯錯誤時擲出。</exception>
        public TResponse Send<TRequest, TResponse>(HttpMethod method, string requestUri, TRequest request)
            where TRequest : class
            where TResponse : class
        {
            var requestMessage = new HttpRequestMessage(method, requestUri);

            if (request != null)
            {
                var content = new ObjectContent<TRequest>(request, this.mediaFormatter);
                requestMessage.Content = content;
            }

            var mediaType = this.mediaFormatter.MediaTypeMappings[0].MediaType.MediaType;
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));

            HttpResponseMessage responseMessage = null;
            try
            {
                responseMessage = this.httpClient.SendAsync(requestMessage).Result;
            }
            catch (AggregateException exception) // AggregateException 屬於多工的例外，因為這裡不使用多工處理，所以應進一步取出原始例外。
            {
                throw exception.InnerException;
            }

            if (responseMessage.IsSuccessStatusCode)
            {
                var response = responseMessage.Content.ReadAsAsync<TResponse>().Result;
                return response;
            }
            else
            {
                HttpError httpError = null;
                try
                {
                    httpError = responseMessage.Content.ReadAsAsync<HttpError>().Result;
                }
                catch (Exception)
                {
                    var message = responseMessage.Content.ReadAsStringAsync().Result;
                    throw new HttpRequestException(message);
                }

                // model state errors
                if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new BadInvocationException(httpError);
                }

                // handled error
                if (responseMessage.StatusCode == HttpStatusCode.NotAcceptable)
                {
                    throw new InvocationNotAcceptableException(httpError);
                }

                // other errors
                throw new HttpServiceException(responseMessage.StatusCode, httpError);
            }
        }

        protected virtual void OnSendingRequest(HttpRequestEventArgs e)
        {
            if (this.SendingRequest != null)
            {
                this.SendingRequest(this, e);
            }
        }

        protected virtual void OnResponseReceived(HttpResponseEventArgs e)
        {
            if (this.ResponseReceived != null)
            {
                this.ResponseReceived(this, e);
            }
        }

        private DelegatingHandler CreateMessageInterceptor()
        {
            var interceptor = new MessageInterceptingHandler();
            interceptor.ProcessingRequest += (sender, e) => this.OnSendingRequest(e);
            interceptor.ProcessingResponse += (sender, e) => this.OnResponseReceived(e);

            return interceptor;
        }
    }
}