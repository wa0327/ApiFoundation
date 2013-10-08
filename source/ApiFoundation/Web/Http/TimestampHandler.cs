using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using ApiFoundation.Security.Cryptography;

namespace ApiFoundation.Web.Http
{
    public sealed class TimestampHandler : DelegatingHandler
    {
        private readonly string absolutePath;
        private readonly ITimestampProvider timestampProvider;

        /// <summary>
        /// Used to insert a global message handler.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="virtualPath"></param>
        /// <param name="timestampProvider"></param>
        public TimestampHandler(HttpConfiguration configuration, string virtualPath, ITimestampProvider timestampProvider)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (virtualPath == null)
            {
                throw new ArgumentNullException("routeTemplate");
            }

            if (timestampProvider == null)
            {
                throw new ArgumentNullException("timestampProvider");
            }

            this.absolutePath = Path.Combine(configuration.VirtualPathRoot, virtualPath);
            this.timestampProvider = timestampProvider;
        }

        /// <summary>
        /// Used for a route handler.
        /// </summary>
        /// <param name="timestampProvider"></param>
        public TimestampHandler(ITimestampProvider timestampProvider)
        {
            if (timestampProvider == null)
            {
                throw new ArgumentNullException("timestampProvider");
            }

            this.timestampProvider = timestampProvider;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(this.absolutePath) || this.IsMatch(request))
            {
                var task = new Task<HttpResponseMessage>(() => this.CreateTimestampResponse(request));
                task.Start();

                return task;
            }

            return base.SendAsync(request, cancellationToken);
        }

        private bool IsMatch(HttpRequestMessage request)
        {
            if (request.RequestUri.AbsolutePath.StartsWith(this.absolutePath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private HttpResponseMessage CreateTimestampResponse(HttpRequestMessage request)
        {
            string timestamp;
            DateTime expires;
            this.timestampProvider.GetTimestamp(out timestamp, out expires);

            var response = new
            {
                Timestamp = timestamp,
                Expires = expires,
            };

            return request.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}