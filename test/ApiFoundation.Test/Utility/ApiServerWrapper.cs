using System;
using System.Diagnostics;
using System.Web.Http.SelfHost;
using ApiFoundation.Net.Http;
using ApiFoundation.Services;

namespace ApiFoundation.Utility
{
    internal sealed class ApiServerWrapper : IDisposable
    {
        internal static void OnRequestReceived(object sender, HttpRequestEventArgs e)
        {
            Trace.TraceInformation("RECV from {0}", e.RequestMessage.Headers.From);

            if (e.RequestMessage.Content != null)
            {
                var header = e.RequestMessage.Content.Headers;
                Trace.TraceInformation("HEADER: {0}", header);

                var raw = e.RequestMessage.Content.ReadAsStringAsync().Result;
                Trace.TraceInformation("RAW: {0}", raw);
            }
        }

        internal static void OnSendingResponse(object sender, HttpResponseEventArgs e)
        {
            Trace.TraceInformation("SEND to {0}", e.ResponseMessage.RequestMessage.Headers.From);

            if (e.ResponseMessage.Content != null)
            {
                var header = e.ResponseMessage.Content.Headers;
                Trace.TraceInformation("HEADER: {0}", header);

                var raw = e.ResponseMessage.Content.ReadAsStringAsync().Result;
                Trace.TraceInformation("RAW: {0}", raw);
            }
        }

        private readonly HttpSelfHostServer server;

        internal HttpSelfHostConfiguration Configuration { get; private set; }

        internal ApiServer Route { get; private set; }

        internal ApiServerWrapper()
        {
            this.Configuration = new HttpSelfHostConfiguration("http://localhost:8591");

            this.Route = new ApiServer(this.Configuration, "API Default", "api/{controller}/{action}", null, null, null);
            this.Route.RequestReceived += ApiServerWrapper.OnRequestReceived;
            this.Route.SendingResponse += ApiServerWrapper.OnSendingResponse;

            this.server = new HttpSelfHostServer(this.Configuration);
            this.server.OpenAsync().Wait();
        }

        public void Dispose()
        {
            this.server.CloseAsync().Wait();
            this.server.Dispose();
        }
    }
}