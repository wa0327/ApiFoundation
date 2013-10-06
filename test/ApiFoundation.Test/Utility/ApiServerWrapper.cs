﻿using System;
using System.Diagnostics;
using System.Web.Http.SelfHost;
using ApiFoundation.Net.Http;
using ApiFoundation.Services;

namespace ApiFoundation.Utility
{
    internal sealed class ApiServerWrapper : IDisposable
    {
        private readonly HttpSelfHostServer server;

        internal ApiServerWrapper(string baseAddress)
        {
            this.Configuration = new HttpSelfHostConfiguration(baseAddress);

            this.Inner = new ApiServer(this.Configuration, "API Default", "api/{controller}/{action}", null, null, null);
            this.Inner.RequestReceived += (sender, e) =>
            {
                Trace.TraceInformation("RECV from {0}", e.RequestMessage.Headers.From);

                if (e.RequestMessage.Content != null)
                {
                    var header = e.RequestMessage.Content.Headers;
                    Trace.TraceInformation("HEADER: {0}", header);

                    var raw = e.RequestMessage.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("RAW: {0}", raw);
                }
            };
            this.Inner.SendingResponse += (sender, e) =>
            {
                Trace.TraceInformation("SEND to {0}", e.ResponseMessage.RequestMessage.Headers.From);

                if (e.ResponseMessage.Content != null)
                {
                    var header = e.ResponseMessage.Content.Headers;
                    Trace.TraceInformation("HEADER: {0}", header);

                    var raw = e.ResponseMessage.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("RAW: {0}", raw);
                }
            };

            this.server = new HttpSelfHostServer(this.Configuration);
            this.server.OpenAsync().Wait();
        }

        internal HttpSelfHostConfiguration Configuration { get; private set; }

        internal ApiServer Inner { get; private set; }

        internal ApiServerWrapper()
            : this("http://localhost:8591")
        {
        }

        public void Dispose()
        {
            this.server.CloseAsync().Wait();
            this.server.Dispose();
        }
    }
}