using System;

namespace ApiFoundation.Net.Http
{
    public sealed class InvalidTimestampException : Exception
    {
        private readonly string timestamp;

        public InvalidTimestampException(string timestamp)
            : base(string.Format("Invalid timestamp '{0}'.", timestamp))
        {
            this.timestamp = timestamp;
        }

        public InvalidTimestampException(string timestamp, Exception innerException)
            : base(string.Format("Invalid timestamp '{0}'.", timestamp), innerException)
        {
            this.timestamp = timestamp;
        }

        public string Timestamp
        {
            get { return this.timestamp; }
        }
    }
}