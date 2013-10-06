using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ApiFoundation.Net.Http
{
    internal sealed class TimestampServiceHandler : DelegatingHandler
    {
        private ITimestampService provider;

        internal ITimestampService TimestampProvider
        {
            get { return this.provider; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("null");
                }

                this.provider = value;
            }
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var source = new TaskCompletionSource<HttpResponseMessage>();

            string timestamp;
            DateTime expires;
            this.provider.GetTimestamp(out timestamp, out expires);

            var response = new
            {
                Timestamp = timestamp,
                Expires = expires,
            };

            var responseMessage = request.CreateResponse(HttpStatusCode.OK, response);
            source.SetResult(responseMessage);

            return source.Task;
        }
    }
}