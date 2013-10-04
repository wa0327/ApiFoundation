using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using ApiFoundation.Net.Http;

namespace ApiFoundation.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient httpClient;
        private MediaTypeFormatter mediaFormatter;

        public ApiClient()
        {
            var handler = this.CreateMessageHandler();

            this.httpClient = new HttpClient(handler, true);
            this.mediaFormatter = new JsonMediaTypeFormatter(); // default is Json.
        }

        public ILogWriter LogWriter
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public MediaTypeFormatter MediaFormatter
        {
            get { return this.mediaFormatter; }
            set { this.mediaFormatter = value; }
        }

        public Uri BaseAddress
        {
            get { return this.httpClient.BaseAddress; }
            set { this.httpClient.BaseAddress = value; }
        }

        public HttpRequestHeaders DefaultRequestHeaders
        {
            get { return this.httpClient.DefaultRequestHeaders; }
        }

        public long MaxResponseContentBufferSize
        {
            get { return this.httpClient.MaxResponseContentBufferSize; }
            set { this.httpClient.MaxResponseContentBufferSize = value; }
        }

        public TimeSpan Timeout
        {
            get { return this.httpClient.Timeout; }
            set { this.httpClient.Timeout = value; }
        }

        public event EventHandler<HttpRequestEventArgs> SendingRequest;

        public event EventHandler<HttpResponseEventArgs> ResponseReceived;

        public void Dispose()
        {
            this.httpClient.Dispose();
        }

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

                // business errors
                if (responseMessage.StatusCode == HttpStatusCode.NotAcceptable)
                {
                    throw new InvocationNotAcceptableException(httpError);
                }

                // other errors
                throw new HttpServiceException(responseMessage.StatusCode, httpError);
            }
        }

        protected virtual void OnSendingRequest(HttpRequestMessage request)
        {
            if (this.SendingRequest != null)
            {
                var e = new HttpRequestEventArgs(request);
                this.SendingRequest(this, e);
            }
        }

        protected virtual void OnResponseReceived(HttpResponseMessage response)
        {
            if (this.ResponseReceived != null)
            {
                var e = new HttpResponseEventArgs(response);
                this.ResponseReceived(this, e);
            }
        }

        private MessageProcessingHandler CreateMessageHandler()
        {
            var interceptor = new MessageInterceptingHandler
            {
                ProcessRequestDelegate = request => this.OnSendingRequest(request),
                ProcessResponseDelegate = response => this.OnResponseReceived(response),
                InnerHandler = new HttpClientHandler(),
            };

            return interceptor;
        }
    }
}