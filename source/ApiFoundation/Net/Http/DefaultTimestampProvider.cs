using System;

namespace ApiFoundation.Net.Http
{
    public class DefaultTimestampProvider : ITimestampProvider
    {
        private readonly TimeSpan duration;

        public DefaultTimestampProvider(TimeSpan duration)
        {
            this.duration = duration;
        }

        void ITimestampProvider.GetTimestamp(out string timestamp, out DateTime expires)
        {
            timestamp = DateTime.UtcNow.Ticks.ToString("d");
            expires = DateTime.UtcNow.Add(this.duration);
        }
    }
}