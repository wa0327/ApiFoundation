using System;
using System.Net.Http;

namespace ApiFoundation.Net.Http
{
    public sealed class HttpRequestEventArgs : EventArgs
    {
        internal HttpRequestEventArgs(HttpRequestMessage requestMessage)
        {
            this.RequestMessage = requestMessage;
        }

        public HttpRequestMessage RequestMessage { get; set; }
    }
}