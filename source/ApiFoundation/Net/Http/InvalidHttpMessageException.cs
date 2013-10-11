using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiFoundation.Net.Http
{
    public sealed class InvalidHttpMessageException : Exception
    {
        public InvalidHttpMessageException(Exception innerException)
            : base("Invalid HTTP message.", innerException)
        {
        }
    }
}