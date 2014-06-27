using System;
using System.Diagnostics;
using ApiFoundation.Net.Http;

namespace ApiFoundation.Web.Http
{
    public class DefaultTimestampProvider : ITimestampProvider<string>
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

        string ITimestampProvider<string>.GetTimestamp()
        {
            return DateTime.UtcNow.Ticks.ToString();
        }

        void ITimestampProvider<string>.Validate(string timestamp)
        {
            DateTime target;
            try
            {
                var ticks = long.Parse(timestamp);
                target = new DateTime(ticks);
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