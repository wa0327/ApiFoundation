using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace ApiFoundation.Net.Http
{
    internal sealed class MessageInterceptingHandler : MessageProcessingHandler
    {
        internal Action<HttpRequestMessage> ProcessRequestDelegate { get; set; }

        internal Action<HttpResponseMessage> ProcessResponseDelegate { get; set; }

        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this.ProcessRequestDelegate != null)
            {
                this.ProcessRequestDelegate(request);
            }

            return request;
        }

        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (this.ProcessResponseDelegate != null)
            {
                this.ProcessResponseDelegate(response);
            }

            return response;
        }
    }
}