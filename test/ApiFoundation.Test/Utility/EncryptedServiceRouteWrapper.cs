using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.SelfHost;
using ApiFoundation.Net.Http;
using ApiFoundation.Security.Cryptography;
using ApiFoundation.Services;

namespace ApiFoundation.Utility
{
    internal sealed class EncryptedServiceRouteWrapper : IDisposable
    {
        private readonly HttpSelfHostServer server;

        internal HttpSelfHostConfiguration Configuration { get; private set; }

        internal EncryptedApiServer Route { get; private set; }

        internal EncryptedServiceRouteWrapper()
        {
            this.Configuration = new HttpSelfHostConfiguration("http://localhost:8591");

            CryptoService cryptoService = new CryptoServiceWrapper();
            ITimestampProvider timestampProvider = new DefaultTimestampProvider(TimeSpan.FromMinutes(15));

            this.Route = new EncryptedApiServer(this.Configuration, "API Default", "api/{controller}/{action}", null, null, null, cryptoService, timestampProvider);
            //this.Route.RequestReceived += ApiServerWrapper.OnRequestReceived;
            //this.Route.SendingResponse += ApiServerWrapper.OnSendingResponse;
            this.Route.DecryptingRequest += (sender, e) =>
            {
                Trace.TraceInformation("DecryptingRequest");

                if (e.RequestMessage.Content != null)
                {
                    var header = e.RequestMessage.Content.Headers;
                    Trace.TraceInformation("HEADER: {0}", header);

                    var raw = e.RequestMessage.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("RAW: {0}", raw);
                }
            };
            this.Route.RequestDecrypted += (sender, e) =>
            {
                Trace.TraceInformation("RequestDecrypted");

                if (e.RequestMessage.Content != null)
                {
                    var header = e.RequestMessage.Content.Headers;
                    Trace.TraceInformation("HEADER: {0}", header);

                    var raw = e.RequestMessage.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("RAW: {0}", raw);
                }
            };
            this.Route.EncryptingResponse += (sender, e) =>
            {
                Trace.TraceInformation("EncryptingResponse");

                if (e.ResponseMessage.Content != null)
                {
                    var header = e.ResponseMessage.Content.Headers;
                    Trace.TraceInformation("HEADER: {0}", header);

                    var raw = e.ResponseMessage.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("RAW: {0}", raw);
                }
            };
            this.Route.ResponseEncrypted += (sender, e) =>
            {
                Trace.TraceInformation("ResponseEncrypted");

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

        public void Dispose()
        {
            this.server.CloseAsync().Wait();
            this.server.Dispose();
        }
    }
}