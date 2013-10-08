using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;
using ApiFoundation.Security.Cryptography;
using ApiFoundation.Web.Http;

namespace ApiFoundation.Utility
{
    internal sealed class EncryptedHttpServerWrapper : IDisposable
    {
        private readonly HttpConfiguration configuration;
        private readonly HttpSelfHostServer inner;

        internal EncryptedHttpServerWrapper(string baseAddress)
        {
            var configuration = new HttpSelfHostConfiguration(baseAddress);

            // arrange.
            var timestampProvider = new DefaultTimestampProvider();
            var cryptoHandler = new ServerCryptoHandler("secretKeyPassword", "initialVectorPassword", "hashKeyString", timestampProvider);

            // register handlers.
            configuration.MessageHandlers.Add(new ServerMessageDumper());
            configuration.MessageHandlers.Add(new TimestampHandler(configuration, "!timestamp!/get", timestampProvider));

            // startup local HTTP server.
            this.inner = new HttpSelfHostServer(configuration);
            this.inner.OpenAsync().Wait();

            this.configuration = configuration;
        }

        internal EncryptedHttpServerWrapper()
            : this("http://localhost:8591")
        {
        }

        internal HttpConfiguration Configuration
        {
            get { return this.configuration; }
        }

        public void Dispose()
        {
            this.inner.CloseAsync().Wait();
            this.inner.Dispose();
        }
    }
}