using System;

namespace ApiFoundation.Security.Cryptography
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