using System;
using System.Net.Http;

namespace ApiFoundation.Net.Http
{
    public sealed class HttpResponseEventArgs : EventArgs
    {
        internal HttpResponseEventArgs(HttpResponseMessage responseMessage)
        {
            this.ResponseMessage = responseMessage;
        }

        public HttpResponseMessage ResponseMessage { get; set; }
    }
}