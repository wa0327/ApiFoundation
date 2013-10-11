using System;
using System.Diagnostics;

namespace ApiFoundation.Net.Http
{
    public class DefaultTimestampProvider : ITimestampProvider<long>
    {
        private TimeSpan duration;

        public DefaultTimestampProvider(TimeSpan duration)
        {
            if (duration == TimeSpan.Zero)
            {
                throw new ArgumentException("Parameter of duration cannot be Zero.", "duration");
            }

            this.duration = duration;
        }

        void IDisposable.Dispose()
        {
        }

        long ITimestampProvider<long>.GetTimestamp()
        {
            return DateTime.UtcNow.Ticks;
        }

        void ITimestampProvider<long>.Validate(long timestamp)
        {
            DateTime target;
            try
            {
                target = new DateTime(timestamp);
                Trace.TraceInformation("Target: {0:yyyy/MM/dd HH:mm:ss.ffff}", target);
            }
            catch (Exception ex)
            {
                throw new InvalidTimestampException(timestamp.ToString(), ex);
            }

            var now = DateTime.UtcNow;
            Trace.TraceInformation("Now: {0:yyyy/MM/dd HH:mm:ss.ffff}", now);

            var expires = target.Add(this.duration);
            Trace.TraceInformation("Expires: {0:yyyy/MM/dd HH:mm:ss.ffff}", expires);

            if (now < target || now >= expires)
            {
                throw new InvalidTimestampException(timestamp.ToString());
            }
        }
    }
}