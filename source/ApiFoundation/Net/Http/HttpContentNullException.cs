using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiFoundation.Net.Http
{
    public sealed class HttpContentNullException : Exception
    {
        public HttpContentNullException()
            : base("HTTP content is null.")
        {
        }
    }
}