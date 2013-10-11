using System;
using System.Web.Http;
using System.Web.Http.SelfHost;
using ApiFoundation.Net.Http;
using ApiFoundation.Web.Http;

namespace ApiFoundation.Web.Http
{
    internal sealed class EncryptedHttpServerWrapper : IDisposable
    {
        private readonly HttpConfiguration configuration;
        private readonly HttpSelfHostServer inner;

        internal EncryptedHttpServerWrapper(string baseAddress)
        {
            var configuration = new HttpSelfHostConfiguration(baseAddress);

            // arrange.
            var timestampProvider = new DefaultTimestampProvider(TimeSpan.FromMinutes(15));
            var cryptoHandler = new ServerCryptoHandler("secretKeyPassword", "initialVectorPassword", "hashKeyString", timestampProvider);

            // register handlers.
            configuration.MessageHandlers.Add(new ServerMessageDumper());
            configuration.MessageHandlers.Add(new HttpTimestampHandler<long>(configuration, "api2/!timestamp!/get", timestampProvider));
            configuration.Routes.MapHttpRoute("Fake Timestamp Route", "api2/!timestamp!/{action}");

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