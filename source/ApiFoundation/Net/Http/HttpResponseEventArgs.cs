using System;
using System.Net.Http;

namespace ApiFoundation.Net.Http
{
    public sealed class HttpResponseEventArgs : EventArgs
    {
        internal HttpResponseEventArgs(HttpResponseMessage response)
        {
            this.ResponseMessage = response;
        }

        public HttpResponseMessage ResponseMessage { get; set; }
    }
}