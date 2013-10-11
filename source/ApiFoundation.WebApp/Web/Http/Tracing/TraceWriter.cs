using System;
using System.Net.Http;
using System.Web.Http.Tracing;

namespace ApiFoundation.Web.Http.Tracing
{
    internal sealed class TraceWriter : ITraceWriter
    {
        public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            var record = new TraceRecord(request, category, level);
            traceAction(record);

            System.Diagnostics.Trace.TraceInformation("{0} {1} {2}", category, level, record.Message);
        }
    }
}