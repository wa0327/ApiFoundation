using System;

namespace ApiFoundation.Net.Http
{
    public sealed class TimestampNullException : Exception
    {
        public TimestampNullException()
            : base("Timestamp is null.")
        {
        }
    }
}