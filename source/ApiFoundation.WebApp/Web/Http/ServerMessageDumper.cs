using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace ApiFoundation.Web.Http
{
    internal sealed class ServerMessageDumper : MessageProcessingHandler
    {
        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("[RECV {0}]", request.RequestUri);

            var content = request.Content;
            if (content != null)
            {
                builder.AppendLine();

                var header = content.Headers.ToString();
                if (!string.IsNullOrEmpty(header))
                {
                    builder.AppendLine(header);
                }

                var raw = content.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(raw))
                {
                    builder.AppendLine(raw);
                }
            }

            Trace.TraceInformation(builder.ToString());

            return request;
        }

        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("[REPLY {0}]", response.RequestMessage.RequestUri);

            var content = response.Content;
            if (content != null)
            {
                builder.AppendLine();

                var header = content.Headers.ToString();
                if (!string.IsNullOrEmpty(header))
                {
                    builder.AppendLine(header);
                }

                var raw = content.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(raw))
                {
                    builder.AppendLine(raw);
                }
            }

            Trace.TraceInformation(builder.ToString());

            return response;
        }
    }
}