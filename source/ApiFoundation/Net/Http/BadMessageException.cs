using System;

namespace ApiFoundation.Net.Http
{
    public sealed class BadMessageException : Exception
    {
        internal BadMessageException(Exception innerException)
            : base("Bad message.", innerException)
        {
        }
    }
}