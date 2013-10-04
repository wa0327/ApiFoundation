using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace ApiFoundation.Services
{
    public sealed class HttpRequestEventArgs : EventArgs
    {
        internal HttpRequestEventArgs(HttpRequestMessage request)
        {
            this.Request = request;
        }

        public HttpRequestMessage Request { get; private set; }
    }
}