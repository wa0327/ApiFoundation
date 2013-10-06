using System;
using System.Diagnostics;
using System.Net.Http;
using ApiFoundation.Net.Http;
using ApiFoundation.Services;

namespace ApiFoundation.Utility
{
    internal sealed class ApiClientWrapper : IDisposable
    {
        private readonly ApiClient inner;

        internal ApiClientWrapper(string baseAddress)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress),
            };

            this.inner = new ApiClient(httpClient);
            this.inner.SendingRequest += (sender, e) =>
            {
                Trace.TraceInformation("SEND to {0}", e.RequestMessage.RequestUri);

                if (e.RequestMessage.Content != null)
                {
                    var header = e.RequestMessage.Content.Headers;
                    Trace.TraceInformation("HEADER: {0}", header);

                    var raw = e.RequestMessage.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("RAW: {0}", raw);
                }
            };
            this.inner.ResponseReceived += (sender, e) =>
            {
                Trace.TraceInformation("RECV from {0}", e.ResponseMessage.RequestMessage.RequestUri);

                if (e.ResponseMessage.Content != null)
                {
                    var header = e.ResponseMessage.Content.Headers;
                    Trace.TraceInformation("HEADER: {0}", header);

                    var raw = e.ResponseMessage.Content.ReadAsStringAsync().Result;
                    Trace.TraceInformation("RAW: {0}", raw);
                }
            };
        }

        internal ApiClientWrapper()
            : this("http://localhost:8591")
        {
        }

        public void Dispose()
        {
            this.inner.Dispose();
        }

        public static implicit operator ApiClient(ApiClientWrapper source)
        {
            return source.inner;
        }
    }
}