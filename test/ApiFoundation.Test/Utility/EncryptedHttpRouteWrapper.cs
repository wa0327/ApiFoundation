using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;
using ApiFoundation.Security.Cryptography;
using ApiFoundation.Web.Http;

namespace ApiFoundation.Utility
{
    internal sealed class EncryptedHttpRouteWrapper : IDisposable
    {
        private readonly HttpConfiguration configuration;
        private readonly HttpSelfHostServer inner;

        internal EncryptedHttpRouteWrapper(string baseAddress)
        {
            var configuration = new HttpSelfHostConfiguration(baseAddress);

            this.RegisterEncryptedRoute(configuration);
            this.RegisterTimestampRoute(configuration);

            // startup local HTTP server.
            this.inner = new HttpSelfHostServer(configuration);
            this.inner.OpenAsync().Wait();

            this.configuration = configuration;
        }

        internal EncryptedHttpRouteWrapper()
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

        private void RegisterEncryptedRoute(HttpConfiguration configuration)
        {
            // arrange.
            var timestampProvider = new DefaultTimestampProvider();
            var cryptoHandler = new ServerCryptoHandler("secretKeyPassword", "initialVectorPassword", "hashKeyString", timestampProvider);
            var injection = new DelegatingHandler[] { new ServerMessageDumper(), cryptoHandler, new ServerMessageDumper() };
            var handler = HttpClientFactory.CreatePipeline(new HttpControllerDispatcher(configuration), injection);

            // register encrypted HTTP route.
            configuration.Routes.MapHttpRoute("Encrypted API", "api2/{controller}/{action}", null, null, handler);
        }

        private void RegisterTimestampRoute(HttpConfiguration configuration)
        {
            // arrange.
            var timestampProvider = new DefaultTimestampProvider();
            var timestampHandler = new TimestampHandler(timestampProvider);
            var injection = new DelegatingHandler[] { new ServerMessageDumper() };
            var handler = HttpClientFactory.CreatePipeline(timestampHandler, injection);

            // register timestamp route.
            configuration.Routes.MapHttpRoute("Timestamp Route", "api3/!timestamp!/get", null, null, handler);
        }
    }
}