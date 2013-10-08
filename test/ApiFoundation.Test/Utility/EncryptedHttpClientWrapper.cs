using System;
using System.Diagnostics;
using System.Net.Http;
using ApiFoundation.Net.Http;
using ApiFoundation.Security.Cryptography;

namespace ApiFoundation.Utility
{
    internal sealed class EncryptedHttpClientWrapper : IDisposable
    {
        private readonly HttpClient inner;

        internal EncryptedHttpClientWrapper(string baseAddress)
        {
            // arrange.
            var baseUri = new Uri(baseAddress);
            var timestampUri = new Uri("api3/!timestamp!/get", UriKind.Relative);
            var timestampProvider = new HttpTimestampProvider(new HttpClient(), new Uri(baseUri, timestampUri));
            var cryptoHandler = new ClientCryptoHandler("secretKeyPassword", "initialVectorPassword", "hashKeyString", timestampProvider);

            // create HTTP client.
            this.inner = HttpClientFactory.Create(new HttpClientHandler(), new ClientMessageDumper(), cryptoHandler, new ClientMessageDumper());
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