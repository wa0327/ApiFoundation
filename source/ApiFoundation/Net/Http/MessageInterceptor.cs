using System;
using System.Net.Http;
using System.Threading;

namespace ApiFoundation.Net.Http
{
    public class MessageInterceptor : MessageProcessingHandler
    {
        public event EventHandler<HttpRequestEventArgs> ProcessingRequest;

        public event EventHandler<HttpResponseEventArgs> ProcessingResponse;

        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var e = new HttpRequestEventArgs(request);
            this.OnProcessingRequest(e);

            return e.RequestMessage;
        }

        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var e = new HttpResponseEventArgs(response);
            this.OnProcessingResponse(e);

            return e.ResponseMessage;
        }

        protected virtual void OnProcessingRequest(HttpRequestEventArgs e)
        {
            if (this.ProcessingRequest != null)
            {
                this.ProcessingRequest(this, e);
            }
        }

        protected virtual void OnProcessingResponse(HttpResponseEventArgs e)
        {
            if (this.ProcessingResponse != null)
            {
                this.ProcessingResponse(this, e);
            }
        }
    }
}