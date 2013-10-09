using System;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace ApiFoundation.Net.Http
{
    public class HttpTimestampProvider<T> : ITimestampProvider<T>
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
            : this(messageInvoker, new Uri(timestampUri, UriKind.RelativeOrAbsolute))
        {
        }

        void IDisposable.Dispose()
        {
            this.messageInvoker.Dispose();
        }

        T ITimestampProvider<T>.GetTimestamp()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, this.timestampUri);
            var response = this.messageInvoker.SendAsync(request, CancellationToken.None).Result;
            var responseContent = response.Content.ReadAsAsync<JObject>().Result;

            return responseContent["Timestamp"].Value<T>();
        }

        void ITimestampProvider<T>.Validate(T timestamp)
        {
            throw new NotSupportedException();
        }
    }
}