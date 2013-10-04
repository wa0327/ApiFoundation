using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace ApiFoundation.Services
{
    public sealed class HttpContentEventArgs : EventArgs
    {
        internal HttpContentEventArgs(HttpContent content)
        {
            this.Content = content;
        }

        public HttpContent Content { get; private set; }
    }
}