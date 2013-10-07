using System;
using System.Diagnostics;
using System.Net.Http;
using ApiFoundation.Security.Cryptography;

namespace ApiFoundation.Net.Http
{
    internal class EncryptedHttpClientWrapper : IDisposable
    {
        private readonly HttpClient inner;

        internal EncryptedHttpClientWrapper(string baseAddress)
        {
            var baseUri = new Uri(baseAddress);
            var timestampUri = new Uri("/!timestamp!/get", UriKind.Relative);
            var timestampProvider = new HttpTimestampProvider(new HttpClient(), new Uri(baseUri, timestampUri));
            var encryptedHandler = new EncryptedHttpClientHandler("secretKeyPassword", "initialVectorPassword", "hashKeyString", timestampProvider);

            this.inner = HttpClientFactory.Create(new HttpClientHandler(), new ClientMessageDumper(), encryptedHandler, new ClientMessageDumper());
            this.inner.BaseAddress = baseUri;
        }

        internal EncryptedHttpClientWrapper()
            : this("http://localhost:8591")
        {
        }

        public void Dispose()
        {
            this.inner.Dispose();
        }

        public static implicit operator HttpClient(EncryptedHttpClientWrapper source)
        {
            return source.inner;
        }
    }
}