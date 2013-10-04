using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace ApiFoundation.Services
{
    public sealed class HttpResponseEventArgs : EventArgs
    {
        internal HttpResponseEventArgs(HttpResponseMessage response)
        {
            this.Response = response;
        }

        public HttpResponseMessage Response { get; private set; }
    }
}