using System;

namespace ApiFoundation.Net.Http
{
    public sealed class InvalidTimestampException : Exception
    {
        public InvalidTimestampException(Exception innerException)
            : base("Invalid timestamp.", innerException)
        {
        }

        public InvalidTimestampException()
            : base("Invalid timestamp.")
        {
        }
    }
}