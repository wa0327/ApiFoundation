using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;
using System.Web.Http.Tracing;
using ApiFoundation.Net.Http;
using ApiFoundation.Web.Http.Tracing;

namespace ApiFoundation.Web.Http
{
    internal sealed class EncryptedHttpRouteWrapper : IDisposable
    {
        private readonly HttpConfiguration configuration;
        private readonly HttpSelfHostServer inner;

        internal EncryptedHttpRouteWrapper(string baseAddress)
        {
            var configuration = new HttpSelfHostConfiguration(baseAddress);

            // configuration.Services.Replace(typeof(ITraceWriter), new TraceWriter());

            ITimestampProvider<string> timestampProvider = new DefaultTimestampProvider(TimeSpan.FromMinutes(15));
            this.RegisterEncryptedRoute(configuration, timestampProvider);
            this.RegisterTimestampRoute(configuration, timestampProvider);

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

        private void RegisterEncryptedRoute(HttpConfiguration configuration, ITimestampProvider<string> timestampProvider)
        {
            // arrange.
            var cryptoHandler = new ServerCryptoHandler("secretKeyPassword", "initialVectorPassword", "hashKeyString", timestampProvider);
            var injection = new DelegatingHandler[] { new ServerMessageDumper(), cryptoHandler, new ServerMessageDumper() };
            var handler = HttpClientFactory.CreatePipeline(new HttpControllerDispatcher(configuration), injection);

            // register encrypted HTTP route.
            configuration.Routes.MapHttpRoute("Encrypted API", "api2/{controller}/{action}", null, null, handler);
        }

        private void RegisterTimestampRoute(HttpConfiguration configuration, ITimestampProvider<string> timestampProvider)
        {
            // arrange.
            var timestampHandler = new HttpTimestampHandler<string>(timestampProvider);
            //var injection = new DelegatingHandler[] { new ServerMessageDumper() };
            //var handler = HttpClientFactory.CreatePipeline(timestampHandler, injection);
            var handler = timestampHandler;

            // register timestamp route.
            configuration.Routes.MapHttpRoute("Timestamp Route", "api3/!timestamp!/get", null, null, handler);
        }
    }
}