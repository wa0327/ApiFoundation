using System.Diagnostics;
using System.Net.Http;
using System.Threading;

namespace ApiFoundation.Web.Http
{
    internal sealed class ServerMessageDumper : MessageProcessingHandler
    {
        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Trace.TraceInformation("[RECV {0}]", request.Headers.From);

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
            Trace.TraceInformation("[REPLY {0}]", response.RequestMessage.Headers.From);

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