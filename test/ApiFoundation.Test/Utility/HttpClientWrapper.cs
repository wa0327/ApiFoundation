using System;
using System.Net.Http;
using ApiFoundation.Net.Http;

namespace ApiFoundation.Utility
{
    internal sealed class HttpClientWrapper : IDisposable
    {
        private readonly HttpClient inner;

        internal HttpClientWrapper(string baseAddress)
        {
            // arrange.
            var baseUri = new Uri(baseAddress);

            // create HTTP client.
            this.inner = HttpClientFactory.Create(new HttpClientHandler(), new ClientMessageDumper());
            this.inner.BaseAddress = baseUri;
        }

        internal HttpClientWrapper()
            : this("http://localhost:8591")
        {
        }

        public void Dispose()
        {
            this.inner.Dispose();
        }

        public static implicit operator HttpClient(HttpClientWrapper source)
        {
            return source.inner;
        }
    }
}