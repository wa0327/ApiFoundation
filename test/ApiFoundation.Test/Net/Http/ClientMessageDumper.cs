using System.Diagnostics;
using System.Net.Http;
using System.Threading;

namespace ApiFoundation.Net.Http
{
    internal sealed class ClientMessageDumper : MessageProcessingHandler
    {
        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Trace.TraceInformation("[SEND {0}]", request.RequestUri);

            if (request.Content != null)
            {
                var header = request.Content.Headers;
                Trace.TraceInformation("[HEADER]\r\n{0}", header);

                var raw = request.Content.ReadAsStringAsync().Result;
                Trace.TraceInformation("[RAW]\r\n{0}", raw);
            }

            return request;
        }

        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            Trace.TraceInformation("[RECV {0}]", response.RequestMessage.RequestUri);

            if (response.Content != null)
            {
                var header = response.Content.Headers;
                Trace.TraceInformation("[HEADER]\r\n{0}", header);

                var raw = response.Content.ReadAsStringAsync().Result;
                Trace.TraceInformation("[RAW]\r\n{0}", raw);
            }

            return response;
        }
    }
}