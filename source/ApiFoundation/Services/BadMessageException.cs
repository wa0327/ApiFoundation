using System;

namespace ApiFoundation.Services
{
    public sealed class BadMessageException : Exception
    {
        internal BadMessageException(Exception innerException)
            : base("Bad message.", innerException)
        {
        }
    }
}