using System;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Security.Cryptography
{
    public class HttpTimestampProvider : ITimestampProvider
    {
        private readonly HttpMessageInvoker messageInvoker;
        private readonly Uri timestampUri;

        public HttpTimestampProvider(HttpMessageInvoker messageInvoker, Uri timestampUri)
        {
            if (messageInvoker == null)
            {
                throw new ArgumentNullException("messageInvoker");
            }

            this.messageInvoker = messageInvoker;
            this.timestampUri = timestampUri;
        }

        public HttpTimestampProvider(HttpMessageInvoker messageInvoker, string timestampUri)
            : this(messageInvoker, new Uri(timestampUri))
        {
        }

        public TimeSpan Duration
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public void GetTimestamp(out string timestamp, out DateTime expires)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, this.timestampUri);
            var response = this.messageInvoker.SendAsync(request, CancellationToken.None).Result;
            var responseContent = response.Content.ReadAsAsync<JObject>().Result;

            timestamp = (string)responseContent["Timestamp"];
            expires = (DateTime)responseContent["Expires"];
        }

        public void Validate(string timestamp)
        {
            throw new NotSupportedException();
        }
    }
}