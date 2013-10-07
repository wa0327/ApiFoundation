using System;
using System.Net.Http;

namespace ApiFoundation.Net.Http
{
    public sealed class HttpRequestEventArgs : EventArgs
    {
        internal HttpRequestEventArgs(HttpRequestMessage request)
        {
            this.RequestMessage = request;
        }

        public HttpRequestMessage RequestMessage { get; set; }
    }
}