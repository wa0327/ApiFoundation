using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;
using ApiFoundation.Security.Cryptography;
using ApiFoundation.Web.Http;

namespace ApiFoundation.Web.Http
{
    internal sealed class HttpServerWrapper : IDisposable
    {
        private readonly HttpConfiguration configuration;
        private readonly HttpSelfHostServer inner;

        internal HttpServerWrapper(string baseAddress)
        {
            var configuration = new HttpSelfHostConfiguration(baseAddress);

            // arrange.
            var injection = new DelegatingHandler[] { new ServerMessageDumper() };
            var handler = HttpClientFactory.CreatePipeline(new HttpControllerDispatcher(configuration), injection);

            // register HTTP route.
            configuration.Routes.MapHttpRoute("API Default", "api/{controller}/{action}", null, null, handler);

            // startup local HTTP server.
            this.inner = new HttpSelfHostServer(configuration);
            this.inner.OpenAsync().Wait();

            this.configuration = configuration;
        }

        internal HttpServerWrapper()
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