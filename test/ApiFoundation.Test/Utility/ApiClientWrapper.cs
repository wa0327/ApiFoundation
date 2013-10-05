using System;
using System.Diagnostics;
using System.Net.Http;
using ApiFoundation.Net.Http;
using ApiFoundation.Services;

namespace ApiFoundation.Utility
{
    internal sealed class ApiClientWrapper : IDisposable
    {
        internal static void OnSendingRequest(object sender, HttpRequestEventArgs e)
        {
            Trace.TraceInformation("SEND to {0}", e.RequestMessage.RequestUri);

            if (e.RequestMessage.Content != null)
            {
                var header = e.RequestMessage.Content.Headers;
                Trace.TraceInformation("HEADER: {0}", header);

                var raw = e.RequestMessage.Content.ReadAsStringAsync().Result;
                Trace.TraceInformation("RAW: {0}", raw);
            }
        }

        internal static void OnResponseReceived(object sender, HttpResponseEventArgs e)
        {
            Trace.TraceInformation("RECV from {0}", e.ResponseMessage.RequestMessage.RequestUri);

            if (e.ResponseMessage.Content != null)
            {
                var header = e.ResponseMessage.Content.Headers;
                Trace.TraceInformation("HEADER: {0}", header);

                var raw = e.ResponseMessage.Content.ReadAsStringAsync().Result;
                Trace.TraceInformation("RAW: {0}", raw);
            }
        }

        private readonly ApiClient client;

        internal ApiClientWrapper()
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:8591"),
            };

            this.client = new ApiClient(httpClient);
            this.client.SendingRequest += ApiClientWrapper.OnSendingRequest;
            this.client.ResponseReceived += ApiClientWrapper.OnResponseReceived;
        }

        public void Dispose()
        {
            this.client.Dispose();
        }

        public static implicit operator ApiClient(ApiClientWrapper source)
        {
            return source.client;
        }
    }
}